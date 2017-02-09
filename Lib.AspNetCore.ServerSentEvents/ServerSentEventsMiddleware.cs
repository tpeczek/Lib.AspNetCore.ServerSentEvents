using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (serverSentEventsService == null)
            {
                throw new ArgumentNullException(nameof(serverSentEventsService));
            }

            _next = next;
            _serverSentEventsService = serverSentEventsService;
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
                context.Response.ContentType = Constants.SSE_CONTENT_TYPE;
                context.Response.Body.Flush();

                ServerSentEventsClient client = new ServerSentEventsClient(context.Response);

                if (_serverSentEventsService.ReconnectInterval.HasValue)
                {
                    await client.ChangeReconnectIntervalAsync(_serverSentEventsService.ReconnectInterval.Value);
                }

                string lastEventId = context.Request.Headers[Constants.LAST_EVENT_ID_HTTP_HEADER];
                if (!String.IsNullOrWhiteSpace(lastEventId))
                {
                    await _serverSentEventsService.OnReconnectAsync(client, lastEventId);
                }

                Guid clientId = _serverSentEventsService.AddClient(client);

                await context.RequestAborted.WaitAsync();

                _serverSentEventsService.RemoveClient(clientId);
            }
            else
            {
                await _next(context);
            }
        }
        #endregion
    }
}
