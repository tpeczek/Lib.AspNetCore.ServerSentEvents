using System;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Options for Server-Sent Events endpoint.
    /// </summary>
    public class ServerSentEventsOptions
    {
        /// <summary>
        /// Initializes new instance of <see cref="ServerSentEventsOptions"/>.
        /// </summary>
        public ServerSentEventsOptions()
        {
            OnPrepareAccept = _ => { };
        }

        /// <summary>
        /// Gets or sets the authorization rules.
        /// </summary>
        public ServerSentEventsAuthorization Authorization { get; set; }

        /// <summary>
        /// Called after the status code and Content-Type  have been set, but before the headers has been written.
        /// This can be used to add or change the response headers.
        /// </summary>
        public Action<HttpResponse> OnPrepareAccept { get; set; }
    }
}
