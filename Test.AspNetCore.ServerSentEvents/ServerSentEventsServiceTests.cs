using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Test.AspNetCore.ServerSentEvents
{
    public class ServerSentEventsServiceTests
    {
        #region Prepare SUT
        private static ServerSentEventsClient PrepareServerSentEventsClient(HttpContext context)
        {
            return new ServerSentEventsClient(Guid.NewGuid(), new ClaimsPrincipal(), context.Response);
        }

        private static Mock<Action<IServerSentEventsService, ServerSentEventsClientConnectedArgs>> PrepareOnClientConnectedMock()
        {
            return new Mock<Action<IServerSentEventsService, ServerSentEventsClientConnectedArgs>>();
        }

        private static ServerSentEventsService PrepareServerSentEventsService(bool dropConnection, Action<IServerSentEventsService, ServerSentEventsClientConnectedArgs> onClientConnected)
        {
            return new ServerSentEventsService(Options.Create(new ServerSentEventsServiceOptions<ServerSentEventsService>
                                                              {
                                                                  OnClientConnecting = (service, args) => args.DropConnection = dropConnection,
                                                                  OnClientConnected = onClientConnected
                                                              }));
        }
        #endregion

        #region Tests
        [Fact]
        public async Task OnConnectAsync_DoesNotCallClientConnected_WhenBusinessLogicIsNotMet()
        {
            var onClientConnectedMock = PrepareOnClientConnectedMock();

            // ARRANGE
            var serverSentEventsService = PrepareServerSentEventsService(true, onClientConnectedMock.Object);

            // ACT
            var context = new DefaultHttpContext();
            await serverSentEventsService.OnConnectAsync(context.Request, PrepareServerSentEventsClient(context));

            // ASSERT
            onClientConnectedMock.Verify(m => m.Invoke(It.IsAny<IServerSentEventsService>(), It.IsAny<ServerSentEventsClientConnectedArgs>()), Times.Never);
        }

        [Fact]
        public async Task OnConnectAsync_DoesNotCallClientConnected_WhenBusinessLogicIsMet()
        {
            var onClientConnectedMock = PrepareOnClientConnectedMock();

            // ARRANGE
            var serverSentEventsService = PrepareServerSentEventsService(false, onClientConnectedMock.Object);

            // ACT
            var context = new DefaultHttpContext();
            await serverSentEventsService.OnConnectAsync(context.Request, PrepareServerSentEventsClient(context));

            // ASSERT
            onClientConnectedMock.Verify(m => m.Invoke(It.IsAny<IServerSentEventsService>(), It.IsAny<ServerSentEventsClientConnectedArgs>()), Times.Once);
        }

        [Fact]
        public async Task OnConnectAsync_ReturnsFalse_WhenBusinessLogicIsNotMet()
        {
            var onClientConnectedMock = PrepareOnClientConnectedMock();

            // ARRANGE
            var serverSentEventsService = PrepareServerSentEventsService(true, onClientConnectedMock.Object);

            // ACT
            var context = new DefaultHttpContext();
            var result = await serverSentEventsService.OnConnectAsync(context.Request, PrepareServerSentEventsClient(context));

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public async Task OnConnectAsync_ReturnsTrue_WhenBusinessLogicIsMet()
        {
            var onClientConnectedMock = PrepareOnClientConnectedMock();

            // ARRANGE
            var serverSentEventsService = PrepareServerSentEventsService(false, onClientConnectedMock.Object);

            // ACT
            var context = new DefaultHttpContext();
            var result = await serverSentEventsService.OnConnectAsync(context.Request, PrepareServerSentEventsClient(context));

            // ASSERT
            Assert.True(result);
        }
        #endregion
    }
}
