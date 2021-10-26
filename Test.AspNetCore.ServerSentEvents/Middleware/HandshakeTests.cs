using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Abstractions;
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
                new NewGuidServerSentEventsClientIdProvider(),
                new NoOpServerSentEventsNoReconnectClientsIdsStore(),
                Mock.Of<TestServerSentEventsService>(),
                Options.Create(options),
                NullLoggerFactory.Instance
            );
        }

        private HttpContext PrepareHttpContext(string acceptHeaderValue)
        {
            HttpContext context = new DefaultHttpContext();

            if (acceptHeaderValue != null)
            {
                context.Request.Headers.Append(ACCEPT_HTTP_HEADER, acceptHeaderValue);
            }

            context.RequestAborted = new CancellationToken(true);

            return context;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_SseRequestWithoutAcceptHeader_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext(null);

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequestWithEventStreamAcceptHeader_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext(SSE_CONTENT_TYPE);

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequestWithNotEventStreamAcceptHeader_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware();
            HttpContext context = PrepareHttpContext("text/plain");

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Null(context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequest_CallsOnPrepareAccept()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(new ServerSentEventsOptions
            {
                OnPrepareAccept = onPrepareAcceptMock.Object
            });

            await serverSentEventsMiddleware.Invoke(PrepareHttpContext(null), null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Once);
        }
        #endregion
    }
}
