using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class ServerSentEventsHelper
    {
        #region Fields
        private static byte[] _sseRetryField = Encoding.UTF8.GetBytes(Constants.SSE_RETRY_FIELD);
        private static byte[] _sseIdField = Encoding.UTF8.GetBytes(Constants.SSE_ID_FIELD);
        private static byte[] _sseEventField = Encoding.UTF8.GetBytes(Constants.SSE_EVENT_FIELD);
        private static byte[] _sseDataField = Encoding.UTF8.GetBytes(Constants.SSE_DATA_FIELD);
        private static byte[] _endOfLine = new byte[] { 13, 10 };
        #endregion

        #region HttpResponse Extensions
        internal static async Task AcceptSse(this HttpResponse response)
        {
            response.ContentType = Constants.SSE_CONTENT_TYPE;
            await response.Body.FlushAsync();
        }

        internal static async Task WriteSseRetryAsync(this HttpResponse response, byte[] reconnectInterval)
        {
            await response.WriteSseEventFieldAsync(_sseRetryField, reconnectInterval);
            await response.WriteSseEventBoundaryAsync();
        }

        internal static async Task WriteSseEventAsync(this HttpResponse response, byte[] data)
        {
            await response.WriteSseEventFieldAsync(_sseDataField, data);
            await response.WriteSseEventBoundaryAsync();
        }

        internal static async Task WriteSseEventAsync(this HttpResponse response, ServerSentEvent serverSentEvent)
        {
            if (!String.IsNullOrWhiteSpace(serverSentEvent.Id))
            {
                await response.WriteSseEventFieldAsync(_sseIdField, Encoding.UTF8.GetBytes(serverSentEvent.Id));
            }

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Type))
            {
                await response.WriteSseEventFieldAsync(_sseEventField, Encoding.UTF8.GetBytes(serverSentEvent.Type));
            }

            if (serverSentEvent.Data != null)
            {
                foreach(string data in serverSentEvent.Data)
                {
                    await response.WriteSseEventFieldAsync(_sseDataField, Encoding.UTF8.GetBytes(data));
                }
            }

            await response.WriteSseEventBoundaryAsync();
        }

        private static async Task WriteSseEventFieldAsync(this HttpResponse response, byte[] field, byte[] data)
        {
            await response.Body.WriteAsync(field, 0, field.Length);
            await response.Body.WriteAsync(data, 0, data.Length);
            await response.Body.WriteAsync(_endOfLine, 0, _endOfLine.Length);
        }

        private static Task WriteSseEventBoundaryAsync(this HttpResponse response)
        {
            return response.Body.WriteAsync(_endOfLine, 0, _endOfLine.Length);
        }
        #endregion
    }
}
