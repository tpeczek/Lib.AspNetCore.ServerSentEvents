namespace Lib.AspNetCore.ServerSentEvents.Internals
{
    internal readonly struct ServerSentEventBytes
    {
        #region Properties
        internal byte[] Bytes { get; }

        internal int BytesCount { get; }
        #endregion

        #region Constructor
        internal ServerSentEventBytes(byte[] bytes, int bytesCount)
            : this()
        {
            Bytes = bytes;
            BytesCount = bytesCount;
        }
        #endregion
    }
}
