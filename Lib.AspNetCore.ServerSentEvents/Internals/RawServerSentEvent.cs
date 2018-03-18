using System;
using System.Text;
using System.Collections.Generic;

namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal readonly struct RawServerSentEvent
    {
        #region Properties
        internal byte[] Id { get; }

        internal byte[] Type { get; }

        internal IReadOnlyList<byte[]> Data { get; }
        #endregion

        #region Constructor
        internal RawServerSentEvent(ServerSentEvent serverSentEvent)
            : this()
        {
            if (!String.IsNullOrWhiteSpace(serverSentEvent.Id))
            {
                Id = Encoding.UTF8.GetBytes(serverSentEvent.Id);
            }

            if (!String.IsNullOrWhiteSpace(serverSentEvent.Type))
            {
                Type = Encoding.UTF8.GetBytes(serverSentEvent.Type);
            }

            if (serverSentEvent.Data != null)
            {
                List<byte[]> data = new List<byte[]>(serverSentEvent.Data.Count);

                foreach (string dataItem in serverSentEvent.Data)
                {
                    if (dataItem != null)
                    {
                        data.Add(Encoding.UTF8.GetBytes(dataItem));
                    }
                }

                Data = data;
            }
        }
        #endregion
    }
}
