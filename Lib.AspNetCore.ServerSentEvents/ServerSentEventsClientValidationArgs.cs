using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents
{
    /// <summary>
    /// Provides data for the <see cref="IServerSentEventsService.ValidationHandler"/> event.
    /// </summary>
    public struct ServerSentEventsClientValidationArgs
    {
        #region Properties
        /// <summary>
        /// Gets the request which has been made in order to establish the connection.
        /// </summary>
        public HttpRequest Request { get; }


        /// <summary>
        /// Gets the response, if necessary to handle the request.
        /// </summary>
        public HttpResponse Response { get; }

        /// <summary>
        /// Gets the identifier of last event which client has received (available if client has reconnected).
        /// </summary>
        public string LastEventId { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        public ServerSentEventsClientValidationArgs(HttpRequest request, HttpResponse response)
            : this()
        {
            Request = request;
            Response = response;
            LastEventId = null;
        }

        /// <summary>
        /// Initializes new instance of data.
        /// </summary>
        /// <param name="request">The request which has been made in order to establish the connection.</param>
        /// <param name="response">The response, if necessary to handle the request.</param>
        /// <param name="lastEventId">The identifier of last event which client has received.</param>
        public ServerSentEventsClientValidationArgs(HttpRequest request, HttpResponse response, string lastEventId)
            : this()
        {
            Request = request;
            Response = response;
            LastEventId = lastEventId;
        }
        #endregion
    }
}
