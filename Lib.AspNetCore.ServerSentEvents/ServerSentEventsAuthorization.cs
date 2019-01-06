using Microsoft.AspNetCore.Authorization;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Defines authorization rules for Server-Sent Events endpoint.
    /// </summary>
    public class ServerSentEventsAuthorization : IAuthorizeData
    {
        /// <summary>
        /// Gets the <see cref="ServerSentEventsAuthorization"/> instance which will result in using the default authorization policy.
        /// </summary>
        public static ServerSentEventsAuthorization Default { get; } = new ServerSentEventsAuthorization();

        /// <summary>
        /// Gets or sets the policy name that determines access to the endpoint.
        /// </summary>
        public string Policy { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of roles that are allowed to access the endpoint.
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// Gets or sets a comma delimited list of schemes from which user information is constructed.
        /// </summary>
        public string AuthenticationSchemes { get; set; }
    }
}
