using System;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class NewGuidServerSentEventsClientIdProvider : IServerSentEventsClientIdProvider
    {
        public Guid AcquireClientId(HttpContext context)
        {
            return Guid.NewGuid();
        }
    }
}
