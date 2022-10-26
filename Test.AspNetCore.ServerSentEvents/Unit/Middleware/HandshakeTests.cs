using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware
{
    public class HandshakeTests
    {
        #region Fields
        private const string SSE_CONTENT_TYPE = "text/event-stream";
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_SseRequestWithoutAcceptHeaderAndAcceptHeaderNotRequired_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: null);

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequestWithoutAcceptHeaderAndAcceptHeaderRequired_DoesNotAccept()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(options: new ServerSentEventsOptions { RequireAcceptHeader = true });
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: null);

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Null(context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequestWithEventStreamAcceptHeader_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: SSE_CONTENT_TYPE);

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequestWithNotEventStreamAcceptHeader_DoesNotAccept()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: "text/plain");

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Null(context.Response.ContentType);
        }

        [Fact]
        public async Task Invoke_SseRequest_CallsOnPrepareAccept()
        {
            Mock<Action<HttpResponse>> onPrepareAcceptMock = new Mock<Action<HttpResponse>>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(options: new ServerSentEventsOptions
            {
                OnPrepareAccept = onPrepareAcceptMock.Object
            });

            await serverSentEventsMiddleware.Invoke(SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: null), null);

            onPrepareAcceptMock.Verify(m => m(It.IsAny<HttpResponse>()), Times.Once);
        }
        #endregion
    }
}
