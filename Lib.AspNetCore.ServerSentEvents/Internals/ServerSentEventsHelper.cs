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

        internal static async Task WriteSseEventAsync(this HttpResponse response, RawServerSentEvent serverSentEvent)
        {
            if (serverSentEvent.Id != null)
            {
                await response.WriteSseEventFieldAsync(_sseIdField, serverSentEvent.Id);
            }

            if (serverSentEvent.Type != null)
            {
                await response.WriteSseEventFieldAsync(_sseEventField, serverSentEvent.Type);
            }

            if (serverSentEvent.Data != null)
            {
                for (int i = 0; i < serverSentEvent.Data.Count; i++)
                {
                    await response.WriteSseEventFieldAsync(_sseDataField, serverSentEvent.Data[i]);
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
