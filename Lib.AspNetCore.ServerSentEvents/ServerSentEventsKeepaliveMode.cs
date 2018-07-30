namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The keepalive sending mode.
    /// </summary>
    public enum ServerSentEventsKeepaliveMode
    {
        /// <summary>
        /// Always send keepalive.
        /// </summary>
        Always,
        /// <summary>
        /// Never send keepalive.
        /// </summary>
        Never,
        /// <summary>
        /// Send keepalive if ANCM is detected.
        /// </summary>
        BehindAncm
    }
}
