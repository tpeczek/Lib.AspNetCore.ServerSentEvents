using System;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Options for <see cref="ServerSentEventsService"/>.
    /// <typeparam name="TServerSentEventsService">The type of <see cref="ServerSentEventsService"/> for which the options will be used.</typeparam>
    /// </summary>
    public class ServerSentEventsServiceOptions<TServerSentEventsService> where TServerSentEventsService : ServerSentEventsService
    {
        internal const string DEFAULT_KEEPALIVE_EVENT_TYPE = "KEEPALIVE";
        internal const int DEFAULT_KEEPALIVE_INTERVAL = 30;

        private int _keepaliveInterval = DEFAULT_KEEPALIVE_INTERVAL;
        private string _keepaliveEventType = DEFAULT_KEEPALIVE_EVENT_TYPE;

        /// <summary>
        /// Gets or sets the keepalive event sending mode.
        /// </summary>
        public ServerSentEventsKeepaliveMode KeepaliveMode { get; set; } = ServerSentEventsKeepaliveMode.Never;

        /// <summary>
        /// Gets or sets the keepalive event interval (in seconds).
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
        /// Gets or sets the keepalive event name.
        /// </summary>
        public string KeepaliveEventType
        {
            get { return _keepaliveEventType; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _keepaliveEventType = value;
            }
        }
    }
}
