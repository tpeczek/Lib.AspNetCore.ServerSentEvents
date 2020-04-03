using System;
using System.Collections.Concurrent;
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

        /// <summary>
        /// A set of key-values pairs to store pieces of information that can be used to select clients when sending events.
        /// </summary>
        ConcurrentDictionary<string, string> Properties { get; }
        #endregion

        #region Methods
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
        #endregion
    }
}
