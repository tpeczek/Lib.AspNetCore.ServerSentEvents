namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ClientConnected"/> event.
    /// </summary>
    public struct ServerSentEventsClientConnectedArgs
    {
        #region Properties
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
        /// <param name="client">The client who has connected.</param>
        public ServerSentEventsClientConnectedArgs(IServerSentEventsClient client)
            : this()
        {
            Client = client;
            LastEventId = null;
        }

        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="client">The client who has connected.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        public ServerSentEventsClientConnectedArgs(IServerSentEventsClient client, string lastEventId)
            : this()
        {
            Client = client;
            LastEventId = lastEventId;
        }
        #endregion
    }
}
