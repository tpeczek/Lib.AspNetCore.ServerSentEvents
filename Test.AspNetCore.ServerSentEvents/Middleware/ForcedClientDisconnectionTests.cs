using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Test.AspNetCore.ServerSentEvents.Middleware
{
    public class ForcedClientDisconnectionTests
    {
        #region Fields
        private const string ACCEPT_HTTP_HEADER = "Accept";
        private const string SSE_CONTENT_TYPE = "text/event-stream";

        private static readonly RequestDelegate NOOP_REQUEST_DELEGATE = (context) => Task.CompletedTask;
        #endregion

        #region Prepare SUT
        private ServerSentEventsService PrepareServerSentEventsService(bool acceptConnection)
        {
            var serverSentEventsServiceMock = new Mock<ServerSentEventsService>();

            serverSentEventsServiceMock.Setup(m => m.OnConnectAsync(It.IsAny<HttpRequest>(), It.IsAny<IServerSentEventsClient>()))
                                       .Returns(Task.FromResult(acceptConnection));

            return serverSentEventsServiceMock.Object;
        }

        private ServerSentEventsMiddleware<ServerSentEventsService> PrepareServerSentEventsMiddleware(ServerSentEventsService serverSentEventsService)
        {
            return new ServerSentEventsMiddleware<ServerSentEventsService>
            (
                NOOP_REQUEST_DELEGATE,
                Mock.Of<IAuthorizationPolicyProvider>(),
                serverSentEventsService,
                Options.Create(new ServerSentEventsOptions())
            );
        }

        private Mock<HttpContext> PrepareHttpContext()
        {
            var context = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            var response = new Mock<HttpResponse>();

            request.SetupGet(m => m.Headers)
                   .Returns(new HeaderDictionary(new Dictionary<string, StringValues>
                                                 {
                                                     {
                                                         ACCEPT_HTTP_HEADER, SSE_CONTENT_TYPE
                                                     }
                                                 }));
            request.SetupGet(m => m.HttpContext)
                   .Returns(context.Object);

            response.Setup(m => m.Body)
                    .Returns(Mock.Of<Stream>());

            response.SetupGet(m => m.HttpContext)
                    .Returns(context.Object);

            context.SetupGet(m => m.Features)
                   .Returns(new FeatureCollection());

            context.Setup(m => m.User)
                   .Returns(new ClaimsPrincipal());

            context.SetupGet(m => m.Request)
                   .Returns(request.Object);

            context.SetupGet(m => m.Response)
                   .Returns(response.Object);

            context.Setup(m => m.RequestAborted)
                   .Returns(new CancellationToken(true));
            
            return context;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_AbortsConnections_WhenBusinessLogicIsNotMet()
        {
            // ARRANGE
            var contextMock = PrepareHttpContext();
            var serverSentEventsService = PrepareServerSentEventsService(false);
            var serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(serverSentEventsService);
            
            // ACT
            await serverSentEventsMiddleware.Invoke(contextMock.Object, null);

            // ASSERT
            contextMock.Verify(m => m.Abort(), Times.Once);
        }

        [Fact]
        public async Task Invoke_DoesNotAbortConnections_WhenBusinessLogicIsMet()
        {
            // ARRANGE
            var contextMock = PrepareHttpContext();
            var serverSentEventsService = PrepareServerSentEventsService(true);
            var serverSentEventsMiddleware = PrepareServerSentEventsMiddleware(serverSentEventsService);

            // ACT
            await serverSentEventsMiddleware.Invoke(contextMock.Object, null);

            // ASSERT
            contextMock.Verify(m => m.Abort(), Times.Never);
        }
        #endregion
    }
}
