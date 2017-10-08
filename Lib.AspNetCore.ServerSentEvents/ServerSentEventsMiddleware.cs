using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Lib.AspNetCore.ServerSentEvents.Internals;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Middleware which provides support for Server-Sent Events protocol.
    /// </summary>
    public class ServerSentEventsMiddleware
    {
        #region Fields
        private readonly RequestDelegate _next;
        private readonly ServerSentEventsService _serverSentEventsService;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of middleware.
        /// </summary>
        /// <param name="next">The next delegate in the pipeline.</param>
        /// <param name="serverSentEventsService">The service which provides operations over Server-Sent Events protocol.</param>
        public ServerSentEventsMiddleware(RequestDelegate next, ServerSentEventsService serverSentEventsService)
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

                context.Response.Headers.Append(Constants.CONTENT_ENCODING_HEADER, Constants.IDENTITY_CONTENT_ENCODING);

                await context.Response.AcceptSse();

                ServerSentEventsClient client = new ServerSentEventsClient(Guid.NewGuid(), context.User, context.Response);

                if (_serverSentEventsService.ReconnectInterval.HasValue)
                {
                    await client.ChangeReconnectIntervalAsync(_serverSentEventsService.ReconnectInterval.Value);
                }

                string lastEventId = context.Request.Headers[Constants.LAST_EVENT_ID_HTTP_HEADER];
                if (!String.IsNullOrWhiteSpace(lastEventId))
                {
                    await _serverSentEventsService.OnReconnectAsync(client, lastEventId);
                }

                _serverSentEventsService.AddClient(client);

                await context.RequestAborted.WaitAsync();

                _serverSentEventsService.RemoveClient(client);
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
        #endregion
    }
}
