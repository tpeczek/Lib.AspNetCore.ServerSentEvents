using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ClientConnecting"/> event.
    /// </summary>
    public class ServerSentEventsClientConnectingArgs
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

        /// <summary>
        /// Indicates if the connection with the client should be dropped as it does not meet some business logic criteria.
        /// </summary>
        public bool DropConnection { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="client">The client who has connected.</param>
        public ServerSentEventsClientConnectingArgs(HttpRequest request, IServerSentEventsClient client)
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
        public ServerSentEventsClientConnectingArgs(HttpRequest request, IServerSentEventsClient client, string lastEventId)
        {
            Request = request;
            Client = client;
            LastEventId = lastEventId;
        }
        #endregion
    }
}
