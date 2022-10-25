namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The kind of content for keepalive.
    /// </summary>
    public enum ServerSentEventsKeepaliveKind
    {
        /// <summary>
        /// The keepalive will be send as a comment.
        /// </summary>
        Comment,
        /// <summary>
        /// The keepalive will be send as an event.
        /// </summary>
        Event
    }
}
