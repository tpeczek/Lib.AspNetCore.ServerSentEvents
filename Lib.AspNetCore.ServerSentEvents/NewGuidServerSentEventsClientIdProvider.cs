using System;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The default provider of identifiers for <see cref="IServerSentEventsClient"/> instances based on <see cref="HttpContext"/>.
    /// This provider creates new GUID every time.
    /// </summary>
    public class NewGuidServerSentEventsClientIdProvider : IServerSentEventsClientIdProvider
    {
        /// <inheritdoc />
        public Guid AcquireClientId(HttpContext context)
        {
            return Guid.NewGuid();
        }
    }
}
