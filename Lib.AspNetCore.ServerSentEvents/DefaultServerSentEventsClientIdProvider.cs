using System;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// The default provider of identifiers for <see cref="IServerSentEventsClient"/> instances based on <see cref="HttpContext"/>.
    /// This provider creates new GUID every time.
    /// </summary>
    public class DefaultServerSentEventsClientIdProvider : IServerSentEventsClientIdProvider
    {
        /// <inheritdoc />
        public Guid GetClientId(HttpContext context)
        {
            return Guid.NewGuid();
        }
    }
}
