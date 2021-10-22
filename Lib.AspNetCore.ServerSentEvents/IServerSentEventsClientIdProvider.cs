using System;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Contract for provider of identifiers for <see cref="IServerSentEventsClient"/> instances based on <see cref="HttpContext"/>.
    /// </summary>
    public interface IServerSentEventsClientIdProvider
    {
        /// <summary>
        /// Acquires an identifier which will be used by <see cref="IServerSentEventsClient"/>.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>The identifier.</returns>
        Guid AcquireClientId(HttpContext context);
    }
}
