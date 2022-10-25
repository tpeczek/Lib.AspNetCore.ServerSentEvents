using System;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Options for <see cref="ServerSentEventsService"/>.
    /// <typeparam name="TServerSentEventsService">The type of <see cref="ServerSentEventsService"/> for which the options will be used.</typeparam>
    /// </summary>
    public class ServerSentEventsServiceOptions<TServerSentEventsService> where TServerSentEventsService : ServerSentEventsService
    {
        private const int DEFAULT_KEEPALIVE_INTERVAL = 30;
        private const string DEFAULT_KEEPALIVE_CONTENT = "KEEPALIVE";

        private int _keepaliveInterval = DEFAULT_KEEPALIVE_INTERVAL;
        private string _keepaliveContent = DEFAULT_KEEPALIVE_CONTENT;

        /// <summary>
        /// Gets or sets the keepalive sending mode.
        /// </summary>
        public ServerSentEventsKeepaliveMode KeepaliveMode { get; set; } = ServerSentEventsKeepaliveMode.BehindAncm;

        /// <summary>
        /// Gets or sets the keepalive interval (in seconds).
        /// </summary>
        public int KeepaliveInterval
        {
            get { return _keepaliveInterval; }

            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _keepaliveInterval = value;
            }
        }

        /// <summary>
        /// Gets or sets the kind of content for keepalive.
        /// </summary>
        public ServerSentEventsKeepaliveKind KeepaliveKind { get; set; } = ServerSentEventsKeepaliveKind.Comment;

        /// <summary>
        /// Gets or sets the content for keepalive. If the <see cref="KeepaliveKind"/> is <see cref="ServerSentEventsKeepaliveKind.Comment"/> it will be the content of the comment. If the <see cref="KeepaliveKind"/> is <see cref="ServerSentEventsKeepaliveKind.Event"/> it will be the type of the event.
        /// </summary>
        public string KeepaliveContent
        {
            get { return _keepaliveContent; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _keepaliveContent = value;
            }
        }

        /// <summary>
        /// Gets or sets the interval after which clients will attempt to reestablish failed connections.
        /// </summary>
        public uint? ReconnectInterval { get; set; } = null;

        /// <summary>
        /// Called when client has connected.
        /// This can be used to add client to groups.
        /// </summary>
        public Action<IServerSentEventsService, ServerSentEventsClientConnectedArgs> OnClientConnected { get; set; }

        /// <summary>
        /// Called when client has disconnected.
        /// </summary>
        public Action<IServerSentEventsService, ServerSentEventsClientDisconnectedArgs> OnClientDisconnected { get; set; }
    }
}
