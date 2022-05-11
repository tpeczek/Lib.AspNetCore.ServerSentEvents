using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Contract for client listening for Server-Sent Events
    /// </summary>
    public interface IServerSentEventsClient
    {
        #region Properties
        /// <summary>
        /// Gets the unique client identifier.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the System.Security.Claims.ClaimsPrincipal for user associated with the client.
        /// </summary>
        ClaimsPrincipal User { get; }

        /// <summary>
        /// Gets the value indicating if client is connected.
        /// </summary>
        bool IsConnected { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Disconnects client.
        /// </summary>
        /// <remarks>
        /// This requires registering implementations of <see cref="IServerSentEventsClientIdProvider"/> and <see cref="IServerSentEventsNoReconnectClientsIdsStore"/>.
        /// </remarks>
        void Disconnect();

        /// <summary>
        /// Disconnects the currently connected request only.
        /// </summary>
        void DisconnectCurrentRequest();

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text);

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text, CancellationToken cancellationToken);

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent);

        /// <summary>
        /// Sends event to client.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a piece of information associated to this client.
        /// </summary>
        /// <typeparam name="T">The type of the property being retrieved.</typeparam>
        /// <param name="name">The name of the property being retrieved.</param>
        /// <returns>The value of the property whose name has been specified if it exists in the set of properties associated to the client. Default otherwise.</returns>
        T GetProperty<T>(string name);

        /// <summary>
        /// Removes a piece of information associated to this client.
        /// </summary>
        /// <typeparam name="T">The type of the property being removed.</typeparam>
        /// <param name="name">The name of the property being removed.</param>
        /// <returns>The value of the property whose name has been specified if it exists in the set of properties associated to the client. Default otherwise.</returns>
        T RemoveProperty<T>(string name);

        /// <summary>
        /// Adds a property to the client so that it can be used to store client related pieces of information.
        /// </summary>
        /// <param name="name">The name of the property being added.</param>
        /// <param name="value">The value of the property being added.</param>
        /// <param name="overwrite">When true and the property already exists, its value will be updated. When false and the property already exists, its value will not be updated.</param>
        /// <returns>True if the property has been added or updated, false otherwise.</returns>
        bool SetProperty(string name, object value, bool overwrite = false);
        #endregion
    }
}
