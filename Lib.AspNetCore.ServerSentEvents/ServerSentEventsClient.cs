using System;
using System.Collections.Concurrent;
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
        private readonly bool _clientDisconnectServicesAvailable;
        private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>();
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

        internal bool PreventReconnect { get; set; } = false;

        internal bool IsRemoved { get; set; } = false;
        #endregion

        #region Constructor
        internal ServerSentEventsClient(Guid id, ClaimsPrincipal user, HttpResponse response, bool clientDisconnectServicesAvailable)
        {
            Id = id;
            User = user ?? throw new ArgumentNullException(nameof(user));

            _response = response ?? throw new ArgumentNullException(nameof(response));
            _clientDisconnectServicesAvailable = clientDisconnectServicesAvailable;
            IsConnected = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Retrieves a piece of information associated to this client. This method is thread safe.
        /// </summary>
        /// <typeparam name="T">The type of the property being retrieved.</typeparam>
        /// <param name="name">The name of the property being retrieved.</param>
        /// <returns>The value of the property whose name has been specified if it exists in the set of properties associated to the client. Default otherwise.</returns>
        public T GetProperty<T>(string name)
        {
            if (_properties.TryGetValue(name, out var value))
            {
                return (T) value;
            }

            return default;
        }

        /// <summary>
        /// Removes a piece of information associated to this client. This method is thread safe.
        /// </summary>
        /// <typeparam name="T">The type of the property being removed.</typeparam>
        /// <param name="name">The name of the property being removed.</param>
        /// <returns>The value of the property whose name has been specified if it exists in the set of properties associated to the client. Default otherwise.</returns>
        public T RemoveProperty<T>(string name)
        {
            if (_properties.TryRemove(name, out var value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// Disconnects client.
        /// </summary>
        /// <remarks>
        /// This requires registering implementations of <see cref="IServerSentEventsClientIdProvider"/> and <see cref="IServerSentEventsNoReconnectClientsIdsStore"/>.
        /// </remarks>
        public async Task Disconnect()
        {
            if (!_clientDisconnectServicesAvailable)
            {
                throw new InvalidOperationException($"Disconnecting a {nameof(ServerSentEventsClient)} requires registering implementations of {nameof(IServerSentEventsClientIdProvider)} and {nameof(IServerSentEventsNoReconnectClientsIdsStore)}.");
            }

            PreventReconnect = true;

            if (IsConnected)
            {
                IsConnected = false;
                await _response.Body.FlushAsync();
#if !NET461
                await _response.CompleteAsync();
#else
                _response.HttpContext.Abort();
#endif
            } 
        }

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

        /// <summary>
        /// Adds a property to the client so that it can be used to store client related pieces of information. This method is thread safe.
        /// </summary>
        /// <param name="name">The name of the property being added.</param>
        /// <param name="value">The value of the property being added.</param>
        /// <param name="overwrite">When true and the property already exists, its value will be updated. When false and the property already exists, its value will not be updated.</param>
        /// <returns>True if the property has been added or updated, false otherwise.</returns>
        public bool SetProperty(string name, object value, bool overwrite = false)
        {
            if (overwrite)
            {
                _properties.AddOrUpdate(name, value, (k, v) => value);
                return true;
            }

            return _properties.TryAdd(name, value);
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
