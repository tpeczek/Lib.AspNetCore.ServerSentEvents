namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class Constants
    {
        internal const string ACCEPT_HTTP_HEADER = "Accept";

        internal const string SSE_CONTENT_TYPE = "text/event-stream";

        internal const string CONTENT_ENCODING_HEADER = "Content-Encoding";

        internal const string IDENTITY_CONTENT_ENCODING = "identity";

        internal const string LAST_EVENT_ID_HTTP_HEADER = "Last-Event-ID";

        internal const string SSE_RETRY_FIELD = "retry: ";

        internal const string SSE_COMMENT_FIELD = ": ";

        internal const string SSE_ID_FIELD = "id: ";

        internal const string SSE_EVENT_FIELD = "event: ";

        internal const string SSE_DATA_FIELD = "data: ";
    }
}
