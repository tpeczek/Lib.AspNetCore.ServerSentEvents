using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Contract for service which provides operations over Server-Sent Events protocol.
    /// </summary>
    public interface IServerSentEventsService
    {
        #region Events
        /// <summary>
        /// Occurs when client has connected.
        /// </summary>
        event EventHandler<ServerSentEventsClientConnectedArgs> ClientConnected;

        /// <summary>
        /// Occurs when client has disconnected.
        /// </summary>
        event EventHandler<ServerSentEventsClientDisconnectedArgs> ClientDisconnected;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        uint? ReconnectInterval { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Gets the client based on the unique client identifier.
        /// </summary>
        /// <param name="clientId">The unique client identifier.</param>
        /// <returns>The client.</returns>
        IServerSentEventsClient GetClient(Guid clientId);

        /// <summary>
        /// Gets all clients.
        /// </summary>
        /// <returns>The clients.</returns>
        IReadOnlyCollection<IServerSentEventsClient> GetClients();

        /// <summary>
        /// Gets clients in the specified group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <returns>The clients in the specified group.</returns>
        IReadOnlyCollection<IServerSentEventsClient> GetClients(string groupName);

        /// <summary>
        /// Adds a client to the specified group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="client">The client to add to a group.</param>
        /// <returns>The task object representing the result of asynchronous operation</returns>
        Task<ServerSentEventsAddToGroupResult> AddToGroupAsync(string groupName, IServerSentEventsClient client);

        /// <summary>
        /// Changes the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task ChangeReconnectIntervalAsync(uint reconnectInterval);

        /// <summary>
        /// Changes the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task ChangeReconnectIntervalAsync(uint reconnectInterval, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text, Func<IServerSentEventsClient, bool> clientPredicate);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, string text);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, string text, Func<IServerSentEventsClient, bool> clientPredicate);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, string text, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="text">The simple text event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, string text, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to clients in group.
        /// </summary>
        /// <param name="groupName">The group name.</param>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="clientPredicate">The function to test each client for a condition.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string groupName, ServerSentEvent serverSentEvent, Func<IServerSentEventsClient, bool> clientPredicate, CancellationToken cancellationToken);

        /// <summary>
        /// Method which is called when client is establishing the connection. The base implementation raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is establishing the connection.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnConnectAsync(HttpRequest request, IServerSentEventsClient client);

        /// <summary>
        /// Method which is called when client is reestablishing the connection. The base implementation raises the <see cref="ClientConnected"/> event.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is reestablishing the connection.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnReconnectAsync(HttpRequest request, IServerSentEventsClient client, string lastEventId);

        /// <summary>
        /// Method which is called when client is disconnecting. The base implementation raises the <see cref="ClientDisconnected"/> event.
        /// </summary>
        /// <param name="request">The original request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who is disconnecting.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnDisconnectAsync(HttpRequest request, IServerSentEventsClient client);
        #endregion
    }
}
