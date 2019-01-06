namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Options for Server-Sent Events endpoint.
    /// </summary>
    public class ServerSentEventsOptions
    {
        /// <summary>
        /// Gets or sets the authorization rules.
        /// </summary>
        public ServerSentEventsAuthorization Authorization { get; set; }
    }
}
