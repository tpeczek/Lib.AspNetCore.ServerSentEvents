using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
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

        /// <summary>
        /// A set of key-values pairs to store pieces of information that can be used to select clients when sending events.
        /// </summary>
        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>();
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
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), cancellationToken);
        }

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), cancellationToken);
        }

        internal Task SendAsync(ServerSentEventBytes serverSentEvent, CancellationToken cancellationToken)
        {
            CheckIsConnected();

            return _response.WriteAsync(serverSentEvent, cancellationToken);
        }

        internal Task ChangeReconnectIntervalAsync(uint reconnectInterval, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetReconnectIntervalBytes(reconnectInterval), cancellationToken);
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
