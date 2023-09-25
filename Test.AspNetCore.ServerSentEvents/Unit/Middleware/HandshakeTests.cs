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

        
        [Theory]
        [InlineData("*/*")]
        [InlineData("text/*")]
        [InlineData(SSE_CONTENT_TYPE)]
        public async Task Invoke_SseRequestWithAcceptedHeader_Accepts(string contentType)
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: contentType);

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
        
        [Fact]
        public async Task Invoke_SseRequestWithMultipleAcceptHeadersWithEventStreamAcceptHeader_Accepts()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: SSE_CONTENT_TYPE+",text/html");

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(SSE_CONTENT_TYPE, context.Response.ContentType);
        }
        
        [Fact]
        public async Task Invoke_SseRequestWithMultipleAcceptHeadersWithNotEventStreamAcceptHeader_DoesNotAccept()
        {
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware();
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(acceptHeaderValue: "text/plain,text/html");

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Null(context.Response.ContentType);
        }
        
        #endregion
    }
}
