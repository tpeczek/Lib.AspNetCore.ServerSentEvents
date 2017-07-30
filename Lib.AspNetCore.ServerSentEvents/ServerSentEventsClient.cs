using System;
using System.Security.Claims;
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

        #region Properties
        /// <summary>
        /// Gets the unique client identifier.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Gets or sets the System.Security.Claims.ClaimsPrincipal for user associated with the client.
        /// </summary>
        public ClaimsPrincipal User { get; private set; }

        /// <summary>
        /// Gets the value indicating if client is connected.
        /// </summary>
        public bool IsConnected { get; internal set; }
        #endregion

        #region Constructor
        internal ServerSentEventsClient(Guid id, ClaimsPrincipal user, HttpResponse response)
        {
            Id = id;
            User = user ?? throw new ArgumentNullException(nameof(user));

            _response = response ?? throw new ArgumentNullException(nameof(response));
            IsConnected = true;
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
            CheckIsConnected();

            return _response.WriteSseEventAsync(text);
        }

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent)
        {
            CheckIsConnected();

            return _response.WriteSseEventAsync(serverSentEvent);
        }

        internal Task ChangeReconnectIntervalAsync(uint reconnectInterval)
        {
            CheckIsConnected();

            return _response.WriteSseRetryAsync(reconnectInterval);
        }

        private void CheckIsConnected()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("The client isn't connected.");
            }
        }
        #endregion
    }
}
