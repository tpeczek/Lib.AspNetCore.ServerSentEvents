using System.Threading.Tasks;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Contract for service which provides operations over Server-Sent Events protocol.
    /// </summary>
    public interface IServerSentEventsService
    {
        #region Properties
        /// <summary>
        /// Gets the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        uint? ReconnectInterval { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Changes the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        /// <param name="reconnectInterval">The reconnect interval.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task ChangeReconnectIntervalAsync(uint reconnectInterval);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="text">The simple text event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(string text);

        /// <summary>
        /// Sends event to all clients.
        /// </summary>
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent);

        /// <summary>
        /// When overriden in delivered class allows for recovery when client has reestablished the connection.
        /// </summary>
        /// <param name="client">The client who has reestablished the connection.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task OnReconnectAsync(IServerSentEventsClient client, string lastEventId);
        #endregion
    }
}
