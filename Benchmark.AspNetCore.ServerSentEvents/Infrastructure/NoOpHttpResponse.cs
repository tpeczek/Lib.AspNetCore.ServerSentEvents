using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Benchmark.AspNetCore.ServerSentEvents.Infrastructure
{
    internal class NoOpHttpResponse : HttpResponse
    {
        #region Properties
        public override HttpContext HttpContext => throw new NotImplementedException();

        public override int StatusCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IHeaderDictionary Headers => throw new NotImplementedException();

        public override Stream Body { get => Stream.Null; set => throw new NotImplementedException(); }

        public override long? ContentLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override string ContentType { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override IResponseCookies Cookies => throw new NotImplementedException();

        public override bool HasStarted => throw new NotImplementedException();
        #endregion

        #region Methods
        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
