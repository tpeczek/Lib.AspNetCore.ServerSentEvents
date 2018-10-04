using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ClientConnected"/> event.
    /// </summary>
    public struct ServerSentEventsClientConnectedArgs
    {
        #region Properties
        /// <summary>
        /// Gets the request which has been made in order to establish the connection.
        /// </summary>
        public HttpRequest Request { get; }

        /// <summary>
        /// Gets the client who has connected.
        /// </summary>
        public IServerSentEventsClient Client { get; }

        /// <summary>
        /// Gets the identifier of last event which client has received (available if client has reconnected).
        /// </summary>
        public string LastEventId { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who has connected.</param>
        public ServerSentEventsClientConnectedArgs(HttpRequest request, IServerSentEventsClient client)
            : this()
        {

            Request = request;
            Client = client;
            LastEventId = null;
        }

        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who has connected.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        public ServerSentEventsClientConnectedArgs(HttpRequest request, IServerSentEventsClient client, string lastEventId)
            : this()
        {

            Request = request;
            Client = client;
            LastEventId = lastEventId;
        }
        #endregion
    }
}
