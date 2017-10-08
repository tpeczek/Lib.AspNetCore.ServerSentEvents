using System;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class ServerSentEventsHelper
    {
        #region Fields
        private static byte[] _sseRetryFieldBytes = Encoding.UTF8.GetBytes(Constants.SSE_RETRY_FIELD);
        private static byte[] _sseIdFieldBytes = Encoding.UTF8.GetBytes(Constants.SSE_ID_FIELD);
        private static byte[] _sseEventFieldBytes = Encoding.UTF8.GetBytes(Constants.SSE_EVENT_FIELD);
        private static byte[] _sseDataFieldBytes = Encoding.UTF8.GetBytes(Constants.SSE_DATA_FIELD);
        private static byte[] _endOfLineBytes = new byte[] { 13, 10 };
        #endregion

        #region HttpResponse Extensions
        internal static async Task AcceptSse(this HttpResponse response)
        {
            response.ContentType = Constants.SSE_CONTENT_TYPE;
            await response.Body.FlushAsync();
        }

        internal static async Task WriteSseRetryAsync(this HttpResponse response, uint reconnectInterval)
        {
            await response.WriteSseEventFieldAsync(_sseRetryFieldBytes, reconnectInterval.ToString(CultureInfo.InvariantCulture));
            await response.WriteSseEventBoundaryAsync();
        }

        internal static async Task WriteSseEventAsync(this HttpResponse response, string text)
        {
            await response.WriteSseEventFieldAsync(_sseDataFieldBytes, text);
            await response.WriteSseEventBoundaryAsync();
        }

        internal static async Task WriteSseEventAsync(this HttpResponse response, ServerSentEvent serverSentEvent)
        {
            if (!String.IsNullOrWhiteSpace(serverSentEvent.Id))
            {
                await response.WriteSseEventFieldAsync(_sseIdFieldBytes, serverSentEvent.Id);
            }

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Type))
            {
                await response.WriteSseEventFieldAsync(_sseEventFieldBytes, serverSentEvent.Type);
            }

            if (serverSentEvent.Data != null)
            {
                foreach(string data in serverSentEvent.Data)
                {
                    await response.WriteSseEventFieldAsync(_sseDataFieldBytes, data);
                }
            }

            await response.WriteSseEventBoundaryAsync();
        }

        private static async Task WriteSseEventFieldAsync(this HttpResponse response, byte[] fieldBytes, string data)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            await response.Body.WriteAsync(fieldBytes, 0, fieldBytes.Length);
            await response.Body.WriteAsync(dataBytes, 0, dataBytes.Length);
            await response.Body.WriteAsync(_endOfLineBytes, 0, _endOfLineBytes.Length);
        }

        private static Task WriteSseEventBoundaryAsync(this HttpResponse response)
        {
            return response.Body.WriteAsync(_endOfLineBytes, 0, _endOfLineBytes.Length);
        }
        #endregion
    }
}
