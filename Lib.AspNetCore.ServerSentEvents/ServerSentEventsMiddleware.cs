using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;
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
        private readonly TServerSentEventsService _serverSentEventsService;
        private readonly ServerSentEventsOptions _serverSentEventsOptions;

        private AuthorizationPolicy _authorizationPolicy;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of middleware.
        /// </summary>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="policyProvider">The service which can provide an <see cref="AuthorizationPolicy" />.</param>
        /// <param name="serverSentEventsService">The service which provides operations over Server-Sent Events protocol.</param>
        /// <param name="serverSentEventsOptions"></param>
        public ServerSentEventsMiddleware(RequestDelegate next, IAuthorizationPolicyProvider policyProvider, TServerSentEventsService serverSentEventsService, IOptions<ServerSentEventsOptions> serverSentEventsOptions)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _policyProvider = policyProvider ?? throw new ArgumentNullException(nameof(policyProvider));
            _serverSentEventsService = serverSentEventsService ?? throw new ArgumentNullException(nameof(serverSentEventsService));
            _serverSentEventsOptions = serverSentEventsOptions?.Value ?? throw new ArgumentNullException(nameof(serverSentEventsOptions));
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

                DisableResponseBuffering(context);

                HandleContentEncoding(context);

                await context.Response.AcceptAsync(_serverSentEventsOptions.OnPrepareAccept);

                ServerSentEventsClient client = new ServerSentEventsClient(Guid.NewGuid(), context.User, context.Response);

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

        private bool CheckAcceptHeader(IHeaderDictionary requestHeaders)
        {
            if (!requestHeaders.ContainsKey(Constants.ACCEPT_HTTP_HEADER))
            {
                return true;
            }

            if (requestHeaders[Constants.ACCEPT_HTTP_HEADER].Count == 0)
            {
                return true;
            }

            if (requestHeaders[Constants.ACCEPT_HTTP_HEADER].Any(acceptHeaderValue => acceptHeaderValue == Constants.SSE_CONTENT_TYPE))
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
#if !NETCOREAPP2_1 && !NET461
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

            await _serverSentEventsService.OnDisconnectAsync(request, client);
        }
        #endregion
    }
}
