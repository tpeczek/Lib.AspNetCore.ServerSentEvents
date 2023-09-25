﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Lib.AspNetCore.ServerSentEvents.Internals;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Middleware which provides support for Server-Sent Events protocol.
    /// <typeparam name="TServerSentEventsService">The type of <see cref="ServerSentEventsService"/> which will be used by the middleware instance.</typeparam>
    /// </summary>
    public class ServerSentEventsMiddleware<TServerSentEventsService> where TServerSentEventsService : ServerSentEventsService
    {
        #region Fields
        private readonly RequestDelegate _next;
        private readonly IAuthorizationPolicyProvider _policyProvider;
        private readonly IServerSentEventsClientIdProvider _serverSentEventsClientIdProvider;
        private readonly IServerSentEventsNoReconnectClientsIdsStore _serverSentEventsNoReconnectClientsIdsStore;
        private readonly TServerSentEventsService _serverSentEventsService;
        private readonly ServerSentEventsOptions _serverSentEventsOptions;
        private readonly ILogger<ServerSentEventsMiddleware<TServerSentEventsService>> _logger;
        private readonly bool _clientDisconnectServicesAvailable = false;

        private AuthorizationPolicy _authorizationPolicy;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of middleware.
        /// </summary>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="policyProvider">The service which can provide an <see cref="AuthorizationPolicy" />.</param>
        /// <param name="serverSentEventsClientIdProvider">The provider of identifiers for <see cref="IServerSentEventsClient"/> instances.</param>
        /// <param name="serverSentEventsNoReconnectClientsIdsStore">The store which keeps identifiers of <see cref="IServerSentEventsClient"/> which shouldn't be allowed to reconnect.</param>
        /// <param name="serverSentEventsService">The service which provides operations over Server-Sent Events protocol.</param>
        /// <param name="serverSentEventsOptions"></param>
        /// <param name="loggerFactory">The logger factory.</param>
        public ServerSentEventsMiddleware(RequestDelegate next, IAuthorizationPolicyProvider policyProvider,
            IServerSentEventsClientIdProvider serverSentEventsClientIdProvider, IServerSentEventsNoReconnectClientsIdsStore serverSentEventsNoReconnectClientsIdsStore,
            TServerSentEventsService serverSentEventsService, IOptions<ServerSentEventsOptions> serverSentEventsOptions,
            ILoggerFactory loggerFactory)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
            _serverSentEventsClientIdProvider = serverSentEventsClientIdProvider ?? throw new ArgumentNullException(nameof(serverSentEventsClientIdProvider));
            _serverSentEventsNoReconnectClientsIdsStore = serverSentEventsNoReconnectClientsIdsStore ?? throw new ArgumentNullException(nameof(serverSentEventsNoReconnectClientsIdsStore));
            _serverSentEventsService = serverSentEventsService ?? throw new ArgumentNullException(nameof(serverSentEventsService));
            _serverSentEventsOptions = serverSentEventsOptions?.Value ?? throw new ArgumentNullException(nameof(serverSentEventsOptions));
            _logger = loggerFactory.CreateLogger<ServerSentEventsMiddleware<TServerSentEventsService>>();

            _clientDisconnectServicesAvailable = (_serverSentEventsClientIdProvider.GetType() != typeof(NewGuidServerSentEventsClientIdProvider))
                                                 && (_serverSentEventsNoReconnectClientsIdsStore.GetType() != typeof(NoOpServerSentEventsNoReconnectClientsIdsStore));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="policyEvaluator">The service which can evaluate an <see cref="AuthorizationPolicy" />.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context, IPolicyEvaluator policyEvaluator)
        {
            if (CheckAcceptHeader(context.Request.Headers))
            {
                if (!await AuthorizeAsync(context, policyEvaluator))
                {
                    return;
                }

                Guid clientId = _serverSentEventsClientIdProvider.AcquireClientId(context);
                if (_serverSentEventsService.IsClientConnected(clientId))
                {
                    _logger.LogWarning("The IServerSentEventsClient with identifier {ClientId} is already connected. The request can't have been accepted.", clientId);
                    return;
                }

                if (await PreventReconnectAsync(clientId, context))
                {
                    return;
                }

                DisableResponseBuffering(context);

                HandleContentEncoding(context);

                await context.Response.AcceptAsync(_serverSentEventsOptions.OnPrepareAccept);

                ServerSentEventsClient client = new ServerSentEventsClient(clientId, context.User, context.Response, _clientDisconnectServicesAvailable);

                if (_serverSentEventsService.ReconnectInterval.HasValue)
                {
                    await client.ChangeReconnectIntervalAsync(_serverSentEventsService.ReconnectInterval.Value, CancellationToken.None);
                }

                await ConnectClientAsync(context.Request, client);

                await context.RequestAborted.WaitAsync();

                await DisconnectClientAsync(context.Request, client);
            }
            else
            {
                await _next(context);
            }
        }

        private static readonly string[] AcceptedHeaders = new[] { "*/*", "text/*", Constants.SSE_CONTENT_TYPE };
        
        private bool CheckAcceptHeader(IHeaderDictionary requestHeaders)
        {
            if (!requestHeaders.ContainsKey(Constants.ACCEPT_HTTP_HEADER))
            {
                return !_serverSentEventsOptions.RequireAcceptHeader;
            }

            if (requestHeaders[Constants.ACCEPT_HTTP_HEADER].Count == 0)
            {
                return !_serverSentEventsOptions.RequireAcceptHeader;
            }
            
            if (requestHeaders.GetCommaSeparatedValues(Constants.ACCEPT_HTTP_HEADER).Any(acceptHeaderValue => AcceptedHeaders.Contains(acceptHeaderValue)))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> AuthorizeAsync(HttpContext context, IPolicyEvaluator policyEvaluator)
        {
            bool authorized = false;

            if (_serverSentEventsOptions.Authorization is null)
            {
                authorized = true;
            }
            else
            {
                if (_authorizationPolicy is null)
                {
                    _authorizationPolicy = await AuthorizationPolicy.CombineAsync(_policyProvider, new[] { _serverSentEventsOptions.Authorization });
                }

                AuthenticateResult authenticateResult = await policyEvaluator.AuthenticateAsync(_authorizationPolicy, context);
                PolicyAuthorizationResult authorizeResult = await policyEvaluator.AuthorizeAsync(_authorizationPolicy, authenticateResult, context, null);

                if (authorizeResult.Challenged)
                {
                    await ChallengeAsync(context);
                }
                else if (authorizeResult.Forbidden)
                {
                    await ForbidAsync(context);
                }
                else
                {
                    authorized = true;
                }
            }

            return authorized;
        }

        private async Task<bool> PreventReconnectAsync(Guid clientId, HttpContext context)
        {
            if (!await _serverSentEventsNoReconnectClientsIdsStore.ContainsClientIdAsync(clientId))
            {
                return false;
            }

            context.Response.PreventReconnect();

            _serverSentEventsClientIdProvider.ReleaseClientId(clientId, context);

            await _serverSentEventsNoReconnectClientsIdsStore.RemoveClientIdAsync(clientId);

            return true;
        }

        private async Task ChallengeAsync(HttpContext context)
        {
            if (_authorizationPolicy.AuthenticationSchemes.Count > 0)
            {
                foreach (string authenticationScheme in _authorizationPolicy.AuthenticationSchemes)
                {
                    await context.ChallengeAsync(authenticationScheme);
                }
            }
            else
            {
                await context.ChallengeAsync();
            }
        }

        private async Task ForbidAsync(HttpContext context)
        {
            if (_authorizationPolicy.AuthenticationSchemes.Count > 0)
            {
                foreach (string authenticationScheme in _authorizationPolicy.AuthenticationSchemes)
                {
                    await context.ForbidAsync(authenticationScheme);
                }
            }
            else
            {
                await context.ForbidAsync();
            }
        }

        private void DisableResponseBuffering(HttpContext context)
        {
#if !NET461
            IHttpResponseBodyFeature responseBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();
            if (responseBodyFeature != null)
            {
                responseBodyFeature.DisableBuffering();
            }
#else
            IHttpBufferingFeature bufferingFeature = context.Features.Get<IHttpBufferingFeature>();
            if (bufferingFeature != null)
            {
                bufferingFeature.DisableResponseBuffering();
            }
#endif
        }

        private void HandleContentEncoding(HttpContext context)
        {
            context.Response.OnStarting(ResponseOnStartingCallback, context);
        }

        private static Task ResponseOnStartingCallback(object context)
        {
            HttpResponse response = ((HttpContext)context).Response;

            if (!response.Headers.ContainsKey(Constants.CONTENT_ENCODING_HEADER))
            {
                response.Headers.Append(Constants.CONTENT_ENCODING_HEADER, Constants.IDENTITY_CONTENT_ENCODING);
            }

            return Task.CompletedTask;
        }

        private async Task ConnectClientAsync(HttpRequest request, ServerSentEventsClient client)
        {
            string lastEventId = request.Headers[Constants.LAST_EVENT_ID_HTTP_HEADER];
            if (!String.IsNullOrWhiteSpace(lastEventId))
            {
                await _serverSentEventsService.OnReconnectAsync(request, client, lastEventId);
            }
            else
            {
                await _serverSentEventsService.OnConnectAsync(request, client);
            }

            _serverSentEventsService.AddClient(client);
        }

        private async Task DisconnectClientAsync(HttpRequest request, ServerSentEventsClient client)
        {
            _serverSentEventsService.RemoveClient(client);

            if (client.PreventReconnect)
            {
                await _serverSentEventsNoReconnectClientsIdsStore.AddClientIdAsync(client.Id);
            }
            else
            {
                _serverSentEventsClientIdProvider.ReleaseClientId(client.Id, request.HttpContext);
            }

            await _serverSentEventsService.OnDisconnectAsync(request, client);
        }
        #endregion
    }
}
