using System;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal static class ServerSentEventsHelper
    {
        #region Fields
        private const byte CR = 13;
        private const byte LF = 10;
        private const int CRLF_LENGTH = 2;

        private static byte[] _sseRetryField = Encoding.UTF8.GetBytes(Constants.SSE_RETRY_FIELD);
        private static byte[] _sseCommentField = Encoding.UTF8.GetBytes(Constants.SSE_COMMENT_FIELD);
        private static byte[] _sseIdField = Encoding.UTF8.GetBytes(Constants.SSE_ID_FIELD);
        private static byte[] _sseEventField = Encoding.UTF8.GetBytes(Constants.SSE_EVENT_FIELD);
        private static byte[] _sseDataField = Encoding.UTF8.GetBytes(Constants.SSE_DATA_FIELD);
        #endregion

        #region Methods
        internal static Task AcceptAsync(this HttpResponse response, Action<HttpResponse> onPrepareAccept)
        {
            response.ContentType = Constants.SSE_CONTENT_TYPE;

            onPrepareAccept(response);

            return response.Body.FlushAsync();
        }

        internal static Task WriteAsync(this HttpResponse response, in ServerSentEventBytes serverSentEvent, CancellationToken cancellationToken)
        {
            return response.Body.WriteAsync(serverSentEvent.Bytes, 0, serverSentEvent.BytesCount, cancellationToken);
        }

        internal static ServerSentEventBytes GetReconnectIntervalBytes(uint reconnectInterval)
        {
            string reconnectIntervalStringified = reconnectInterval.ToString(CultureInfo.InvariantCulture);

            byte[] bytes = new byte[GetFieldMaxBytesCount(_sseRetryField, reconnectIntervalStringified) + CRLF_LENGTH];
            int bytesCount = GetFieldBytes(_sseRetryField, reconnectIntervalStringified, bytes, 0);

            bytes[bytesCount++] = CR;
            bytes[bytesCount++] = LF;

            return new ServerSentEventBytes(bytes, bytesCount);
        }

        internal static ServerSentEventBytes GetCommentBytes(string comment)
        {
            byte[] bytes = new byte[GetFieldMaxBytesCount(_sseCommentField, comment) + CRLF_LENGTH];
            int bytesCount = GetFieldBytes(_sseCommentField, comment, bytes, 0);

            bytes[bytesCount++] = CR;
            bytes[bytesCount++] = LF;

            return new ServerSentEventBytes(bytes, bytesCount);
        }

        internal static ServerSentEventBytes GetEventBytes(string text)
        {
            byte[] bytes = new byte[GetFieldMaxBytesCount(_sseDataField, text) + CRLF_LENGTH];
            int bytesCount = GetFieldBytes(_sseDataField, text, bytes, 0);

            bytes[bytesCount++] = CR;
            bytes[bytesCount++] = LF;

            return new ServerSentEventBytes(bytes, bytesCount);
        }

        internal static ServerSentEventBytes GetEventBytes(ServerSentEvent serverSentEvent)
        {
            int bytesCount = 0;
            byte[] bytes = new byte[GetEventMaxBytesCount(serverSentEvent)];

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Id))
            {
                bytesCount = GetFieldBytes(_sseIdField, serverSentEvent.Id, bytes, bytesCount);
            }

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Type))
            {
                bytesCount = GetFieldBytes(_sseEventField, serverSentEvent.Type, bytes, bytesCount);
            }

            if (serverSentEvent.Data != null)
            {
                for (int dataItemIndex = 0; dataItemIndex < serverSentEvent.Data.Count; dataItemIndex++)
                {
                    if (serverSentEvent.Data[dataItemIndex] != null)
                    {
                        bytesCount = GetFieldBytes(_sseDataField, serverSentEvent.Data[dataItemIndex], bytes, bytesCount);
                    }
                }
            }

            bytes[bytesCount++] = CR;
            bytes[bytesCount++] = LF;

            return new ServerSentEventBytes(bytes, bytesCount);
        }

        private static int GetEventMaxBytesCount(ServerSentEvent serverSentEvent)
        {
            int bytesCount = CRLF_LENGTH;

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Id))
            {
                bytesCount += GetFieldMaxBytesCount(_sseIdField, serverSentEvent.Id);
            }

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Type))
            {
                bytesCount += GetFieldMaxBytesCount(_sseEventField, serverSentEvent.Type);
            }

            if (serverSentEvent.Data != null)
            {
                for (int dataItemIndex = 0; dataItemIndex < serverSentEvent.Data.Count; dataItemIndex++)
                {
                    if (serverSentEvent.Data[dataItemIndex] != null)
                    {
                        bytesCount += GetFieldMaxBytesCount(_sseDataField, serverSentEvent.Data[dataItemIndex]);
                    }
                }
            }

            return bytesCount;
        }

        private static int GetFieldBytes(byte[] field, string data, byte[] bytes, int bytesCount)
        {
            for (int fieldIndex = 0; fieldIndex < field.Length; fieldIndex++)
            {
                bytes[bytesCount++] = field[fieldIndex];
            }

            bytesCount += Encoding.UTF8.GetBytes(data, 0, data.Length, bytes, bytesCount);

            bytes[bytesCount++] = CR;
            bytes[bytesCount++] = LF;

            return bytesCount;
        }

        private static int GetFieldMaxBytesCount(byte[] field, string data)
        {
            return field.Length + Encoding.UTF8.GetMaxByteCount(data.Length) + CRLF_LENGTH;
        }
        #endregion
    }
}
