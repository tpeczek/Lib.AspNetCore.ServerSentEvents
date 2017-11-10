namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ClientDisconnected"/> event.
    /// </summary>
    public struct ServerSentEventsClientDisconnectedArgs
    {
        #region Properties
        /// <summary>
        /// Gets the client who has disconnected.
        /// </summary>
        public IServerSentEventsClient Client { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="client">The client who has disconnected.</param>
        public ServerSentEventsClientDisconnectedArgs(IServerSentEventsClient client)
            : this()
        {
            Client = client;
        }
        #endregion
    }
}
