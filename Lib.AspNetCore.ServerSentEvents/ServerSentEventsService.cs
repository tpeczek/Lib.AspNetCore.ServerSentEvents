using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Service which provides operations over Server-Sent Events protocol.
    /// </summary>
    public class ServerSentEventsService : IServerSentEventsService
    {
        #region Events
        /// <summary>
        /// Occurs when client has connected.
        /// </summary>
        public event EventHandler<ServerSentEventsClientConnectedArgs> ClientConnected;

        /// <summary>
        /// Occurs when client has disconnected.
        /// </summary>
        public event EventHandler<ServerSentEventsClientDisconnectedArgs> ClientDisconnected;
        #endregion

        #region Fields
        private readonly ConcurrentDictionary<Guid, ServerSentEventsClient> _clients = new ConcurrentDictionary<Guid, ServerSentEventsClient>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        public uint? ReconnectInterval { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the client based on the unique client identifier.
        /// </summary>
        /// <param name="clientId">The unique client identifier.</param>
        /// <returns>The client.</returns>
        public IServerSentEventsClient GetClient(Guid clientId)
        {
            ServerSentEventsClient client;

            _clients.TryGetValue(clientId, out client);

            return client;
        }

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <returns>The clients.</returns>
        public IReadOnlyCollection<IServerSentEventsClient> GetClients()
        {
            return _clients.Values.ToArray();
        }

        /// <summary>
        /// Changes the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task ChangeReconnectIntervalAsync(uint reconnectInterval)
        {
            return ChangeReconnectIntervalAsync(reconnectInterval, CancellationToken.None);
        }

        /// <summary>
        /// Changes the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task ChangeReconnectIntervalAsync(uint reconnectInterval, CancellationToken cancellationToken)
        {
            ReconnectInterval = reconnectInterval;

            ServerSentEventBytes reconnectIntervalBytes = ServerSentEventsHelper.GetReconnectIntervalBytes(reconnectInterval);

            return ForAllClientsAsync(client => client.SendAsync(reconnectIntervalBytes, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text)
        {
            return SendEventAsync(ServerSentEventsHelper.GetEventBytes(text), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text, CancellationToken cancellationToken)
        {
            return SendEventAsync(ServerSentEventsHelper.GetEventBytes(text), cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent)
        {
            return SendEventAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken)
        {
            return SendEventAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), cancellationToken);
        }

        /// <summary>
        /// Method which is called when client is establishing the connection. The base implementation raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is establishing the connection.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public virtual Task OnConnectAsync(HttpRequest request, IServerSentEventsClient client)
        {
            ClientConnected?.Invoke(this, new ServerSentEventsClientConnectedArgs(request, client));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method which is called when client is reestablishing the connection. The base implementation raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is reestablishing the connection.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public virtual Task OnReconnectAsync(HttpRequest request, IServerSentEventsClient client, string lastEventId)
        {
            ClientConnected?.Invoke(this, new ServerSentEventsClientConnectedArgs(request, client, lastEventId));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Method which is called when client is disconnecting. The base implementation raises the <see cref="ClientDisconnected"/> event.
        /// </summary>
        /// <param name="request">The original request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is disconnecting.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public virtual Task OnDisconnectAsync(HttpRequest request, IServerSentEventsClient client)
        {
            ClientDisconnected?.Invoke(this, new ServerSentEventsClientDisconnectedArgs(request, client));

            return Task.CompletedTask;
        }

        internal void AddClient(ServerSentEventsClient client)
        {
            _clients.TryAdd(client.Id, client);
        }

        internal void RemoveClient(ServerSentEventsClient client)
        {
            client.IsConnected = false;

            _clients.TryRemove(client.Id, out client);
        }

        internal Task SendEventAsync(ServerSentEventBytes serverSentEventBytes, CancellationToken cancellationToken)
        {
            return ForAllClientsAsync(client => client.SendAsync(serverSentEventBytes, cancellationToken), cancellationToken);
        }

        private Task ForAllClientsAsync(Func<ServerSentEventsClient, Task> clientOperationAsync, CancellationToken cancellationToken)
        {
            List<Task> clientsTasks = null;

            foreach (ServerSentEventsClient client in _clients.Values)
            {
                if (client.IsConnected)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Task operationTask = clientOperationAsync(client);

                    if (operationTask.Status != TaskStatus.RanToCompletion)
                    {
                        if (clientsTasks is null)
                        {
                            clientsTasks = new List<Task>();
                        }

                        clientsTasks.Add(operationTask);
                    }
                }
            }

            return (clientsTasks is null) ? Task.CompletedTask : Task.WhenAll(clientsTasks);
        }
        #endregion
    }
}
