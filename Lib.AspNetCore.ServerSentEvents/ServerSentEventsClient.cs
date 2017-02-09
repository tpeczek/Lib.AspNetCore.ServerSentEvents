using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    /// <summary>
    /// Represents client listening for Server-Sent Events
    /// </summary>
    public sealed class ServerSentEventsClient : IServerSentEventsClient
    {
        #region Fields
        private readonly HttpResponse _response;
        #endregion

        #region Constructor
        internal ServerSentEventsClient(HttpResponse response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            _response = response;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text)
        {
            return _response.WriteSseEventAsync(text);
        }

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent)
        {
            return _response.WriteSseEventAsync(serverSentEvent);
        }

        internal Task ChangeReconnectIntervalAsync(uint reconnectInterval)
        {
            return _response.WriteSseRetryAsync(reconnectInterval);
        }
        #endregion
    }
}
