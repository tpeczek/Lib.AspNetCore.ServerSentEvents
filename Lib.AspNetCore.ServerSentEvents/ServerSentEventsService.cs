using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Lib.AspNetCore.ServerSentEvents.Internals;

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
        private readonly ConcurrentDictionary<Guid, IServerSentEventsClient> _clients = new ConcurrentDictionary<Guid, IServerSentEventsClient>();

        private readonly SemaphoreSlim _groupsSemaphore = new SemaphoreSlim(1, 1);
        private static readonly IReadOnlyCollection<IServerSentEventsClient> _emptyGroup = new IServerSentEventsClient[0];
        private readonly Dictionary<string, ConcurrentDictionary<Guid, IServerSentEventsClient>> _groups = new Dictionary<string, ConcurrentDictionary<Guid, IServerSentEventsClient>>();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes new instance of <see cref="ServerSentEventsService"/>.
        /// </summary>
        /// <param name="options">The options for the instance.</param>
        public ServerSentEventsService(IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> options)
        {
            ServerSentEventsServiceOptions<ServerSentEventsService> serviceOptions = options?.Value;

            if (serviceOptions != null)
            {
                ReconnectInterval = serviceOptions.ReconnectInterval;

                if (serviceOptions.OnClientConnected != null)
                {
                    ClientConnected += (sender, args) => serviceOptions.OnClientConnected((IServerSentEventsService)sender, args);
                }

                if (serviceOptions.OnClientDisconnected != null)
                {
                    ClientDisconnected += (sender, args) => serviceOptions.OnClientDisconnected((IServerSentEventsService)sender, args);
                }
            }
        }
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
            IServerSentEventsClient client;

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
        /// Gets clients in the specified group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <returns>The clients in the specified group.</returns>
        public IReadOnlyCollection<IServerSentEventsClient> GetClients(string groupName)
        {
            if (_groups.ContainsKey(groupName))
            {
                return _groups[groupName].Values.ToArray();
            }

            return _emptyGroup;
        }

        /// <summary>
        /// Adds a client to the specified group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// /// <param name="client">The client to add to a group.</param>
        /// <returns>The task object representing the result of asynchronous operation</returns>
        public async Task<ServerSentEventsAddToGroupResult> AddToGroupAsync(string groupName, IServerSentEventsClient client)
        {
            ServerSentEventsAddToGroupResult result = ServerSentEventsAddToGroupResult.AddedToExistingGroup;

            if (!_groups.ContainsKey(groupName))
            {
                await CreateGroupAsync(groupName);

                result = ServerSentEventsAddToGroupResult.AddedToNewGroup;
            }

            _groups[groupName].TryAdd(client.Id, client);

            return result;
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

            return SendAsync(reconnectIntervalBytes, cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text, Func<IServerSentEventsClient, bool> clientPredicate)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), clientPredicate, CancellationToken.None);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, string text)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(text), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, string text, Func<IServerSentEventsClient, bool> clientPredicate)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(text), clientPredicate, CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string text, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(text), clientPredicate, cancellationToken);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, string text, CancellationToken cancellationToken)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(text), cancellationToken);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, string text, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(text), clientPredicate, cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), clientPredicate, CancellationToken.None);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(serverSentEvent), CancellationToken.None);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(serverSentEvent), clientPredicate, CancellationToken.None);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), cancellationToken);
        }

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            return SendAsync(ServerSentEventsHelper.GetEventBytes(serverSentEvent), clientPredicate, cancellationToken);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, CancellationToken cancellationToken)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(serverSentEvent), cancellationToken);
        }

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            return SendAsync(groupName, ServerSentEventsHelper.GetEventBytes(serverSentEvent), clientPredicate, cancellationToken);
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

            _clients.TryRemove(client.Id, out _);

            foreach(ConcurrentDictionary<Guid, IServerSentEventsClient> group in _groups.Values)
            {
                group.TryRemove(client.Id, out _);
            }
        }

        private async Task CreateGroupAsync(string groupName)
        {
            await _groupsSemaphore.WaitAsync();
            try
            {
                if (!_groups.ContainsKey(groupName))
                {
                    _groups.Add(groupName, new ConcurrentDictionary<Guid, IServerSentEventsClient>());
                }
            }
            finally
            {
                _groupsSemaphore.Release();
            }
        }

        internal Task SendAsync(ServerSentEventBytes serverSentEventBytes, CancellationToken cancellationToken)
        {
            return SendAsync(_clients.Values, serverSentEventBytes, cancellationToken);
        }

        internal Task SendAsync(ServerSentEventBytes serverSentEventBytes, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            return SendAsync(_clients.Values.Where(clientPredicate), serverSentEventBytes, cancellationToken);
        }

        internal Task SendAsync(string groupName, ServerSentEventBytes serverSentEventBytes, CancellationToken cancellationToken)
        {
            if (_groups.ContainsKey(groupName))
            {
                return SendAsync(_groups[groupName].Values, serverSentEventBytes, cancellationToken);
            }

            return Task.CompletedTask;
        }

        internal Task SendAsync(string groupName, ServerSentEventBytes serverSentEventBytes, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken)
        {
            if (_groups.ContainsKey(groupName))
            {
                return SendAsync(_groups[groupName].Values.Where(clientPredicate), serverSentEventBytes, cancellationToken);
            }

            return Task.CompletedTask;
        }

        internal Task SendAsync(IEnumerable<IServerSentEventsClient> clients, ServerSentEventBytes serverSentEventBytes, CancellationToken cancellationToken)
        {
            List<Task> clientsTasks = null;

            foreach (ServerSentEventsClient client in clients)
            {
                if (client.IsConnected)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    Task operationTask = client.SendAsync(serverSentEventBytes, cancellationToken);

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
