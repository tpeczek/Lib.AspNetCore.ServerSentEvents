using System.Collections.Generic;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Represents Server-Sent Event.
    /// </summary>
    public class ServerSentEvent
    {
        #region Properties
        /// <summary>
        /// Gets or sets the event identifier (optional).
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the event type (optional).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the event payload.
        /// </summary>
        public IList<string> Data { get; set; }
        #endregion
    }
}
