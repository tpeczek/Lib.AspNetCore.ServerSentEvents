using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        private readonly TServerSentEventsService _serverSentEventsService;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of middleware.
        /// </summary>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="serverSentEventsService">The service which provides operations over Server-Sent Events protocol.</param>
        public ServerSentEventsMiddleware(RequestDelegate next, TServerSentEventsService serverSentEventsService)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _serverSentEventsService = serverSentEventsService ?? throw new ArgumentNullException(nameof(serverSentEventsService));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Process an individual request.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Headers[Constants.ACCEPT_HTTP_HEADER] == Constants.SSE_CONTENT_TYPE)
            {
                DisableResponseBuffering(context);

                HandleContentEncoding(context);

                await context.Response.AcceptAsync();

                ServerSentEventsClient client = new ServerSentEventsClient(Guid.NewGuid(), context.User, context.Response);

                if (_serverSentEventsService.ReconnectInterval.HasValue)
                {
                    await client.ChangeReconnectIntervalAsync(_serverSentEventsService.ReconnectInterval.Value);
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

        private void DisableResponseBuffering(HttpContext context)
        {
            IHttpBufferingFeature bufferingFeature = context.Features.Get<IHttpBufferingFeature>();
            if (bufferingFeature != null)
            {
                bufferingFeature.DisableResponseBuffering();
            }
        }

        private void HandleContentEncoding(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(Constants.CONTENT_ENCODING_HEADER))
                {
                    context.Response.Headers.Append(Constants.CONTENT_ENCODING_HEADER, Constants.IDENTITY_CONTENT_ENCODING);
                }

                return Task.CompletedTask;
            });
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
