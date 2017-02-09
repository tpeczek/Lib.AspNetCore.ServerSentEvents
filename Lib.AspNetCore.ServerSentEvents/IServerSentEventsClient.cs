using System.Threading.Tasks;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Contract for client listening for Server-Sent Events
    /// </summary>
    public interface IServerSentEventsClient
    {
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
        /// <param name="serverSentEvent">The event.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        Task SendEventAsync(ServerSentEvent serverSentEvent);
        #endregion
    }
}
