using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ClientDisconnected"/> event.
    /// </summary>
    public struct ServerSentEventsClientDisconnectedArgs
    {
        #region Properties
        /// <summary>
        /// Gets the original request which has been made in order to establish the connection.
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Gets the client who has disconnected.
        /// </summary>
        public IServerSentEventsClient Client { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The original request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who has disconnected.</param>
        public ServerSentEventsClientDisconnectedArgs(HttpRequest request, IServerSentEventsClient client)
            : this()
        {
            Request = request;
            Client = client;
        }
        #endregion
    }
}
