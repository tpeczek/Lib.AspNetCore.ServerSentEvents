namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class Constants
    {
        internal static string ACCEPT_HTTP_HEADER = "Accept";

        internal static string LAST_EVENT_ID_HTTP_HEADER = "Last-Event-ID";

        internal static string SSE_CONTENT_TYPE = "text/event-stream";

        internal static string SSE_RETRY_FIELD = "retry: ";

        internal static string SSE_ID_FIELD = "id: ";

        internal static string SSE_EVENT_FIELD = "event: ";

        internal static string SSE_DATA_FIELD = "data: ";
    }
}
