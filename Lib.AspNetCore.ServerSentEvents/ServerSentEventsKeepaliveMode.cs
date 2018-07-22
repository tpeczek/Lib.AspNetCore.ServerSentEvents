namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The keepalive event sending mode.
    /// </summary>
    public enum ServerSentEventsKeepaliveMode
    {
        /// <summary>
        /// Always send keepalive event.
        /// </summary>
        Always,
        /// <summary>
        /// Never send keepalive event.
        /// </summary>
        Never
    }
}
