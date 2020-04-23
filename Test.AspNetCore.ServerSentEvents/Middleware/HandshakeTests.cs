using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Middleware
{
    public class HandshakeTests
    {
        #region Fields
        private const string ACCEPT_HTTP_HEADER = "Accept";
        private const string SSE_CONTENT_TYPE = "text/event-stream";

        private static readonly RequestDelegate NOOP_REQUEST_DELEGATE = (context) => Task.CompletedTask;
        #endregion

        #region Prepare SUT
        private ServerSentEventsMiddleware<ServerSentEventsService> PrepareServerSentEventsMiddleware(ServerSentEventsOptions options = null)
        {
            options = options ?? new ServerSentEventsOptions();

            return new ServerSentEventsMiddleware<ServerSentEventsService>
            (
                NOOP_REQUEST_DELEGATE,
                Mock.Of<IAuthorizationPolicyProvider>(),
                Mock.Of<TestServerSentEventsService>(),
                Options.Create(options)
            );
        }

        private HttpContext PrepareHttpContext()
        {
            HttpContext context = new DefaultHttpContext();

            context.Request.Headers.Append(ACCEPT_HTTP_HEADER, SSE_CONTENT_TYPE);
            context.RequestAborted = new CancellationToken(true);

            return context;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_SseRequest_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequest_CallsOnPrepareAccept()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(new ServerSentEventsOptions
            {
                OnPrepareAccept = onPrepareAcceptMock.Object
            });

            await serverSentEventsMiddleware.Invoke(PrepareHttpContext(), null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Once);
        }
        #endregion
    }
}
