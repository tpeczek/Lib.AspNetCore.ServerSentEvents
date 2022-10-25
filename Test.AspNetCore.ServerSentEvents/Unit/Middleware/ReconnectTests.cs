using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware
{
    public class ReconnectTests
    {
        #region Fields
        private static readonly Guid CLIENT_ID = Guid.Parse("90b0401d-9ac7-45bc-841f-04c5b7f9496a");
        #endregion

        #region Prepare SUT
        private Mock<IServerSentEventsClientIdProvider> PrepareServerSentEventsClientIdProviderMock()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = new Mock<IServerSentEventsClientIdProvider>();
            serverSentEventsClientIdProviderMock.Setup(m => m.AcquireClientId(It.IsAny<HttpContext>())).Returns(CLIENT_ID);

            return serverSentEventsClientIdProviderMock;
        }

        private Mock<IServerSentEventsNoReconnectClientsIdsStore> PrepareServerSentEventsNoReconnectClientsIdsStoreMock()
        {
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = new Mock<IServerSentEventsNoReconnectClientsIdsStore>();
            serverSentEventsNoReconnectClientsIdsStoreMock.Setup(m => m.ContainsClientIdAsync(It.IsAny<Guid>()))
                .Returns((Guid clientId) => Task.FromResult(clientId == CLIENT_ID));

            return serverSentEventsNoReconnectClientsIdsStoreMock;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_NoReconnectClientsIdsStoreContainsClientId_ResponseStatusCodeIsStatusIs204NoContent()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = PrepareServerSentEventsNoReconnectClientsIdsStoreMock(); ;
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);
        }

        [Fact]
        public async Task Invoke_NoReconnectClientsIdsStoreContainsClientId_ReleasesClientId()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = PrepareServerSentEventsNoReconnectClientsIdsStoreMock(); ;
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);

            serverSentEventsClientIdProviderMock.Verify(o => o.ReleaseClientId(It.Is<Guid>(clientId => clientId == CLIENT_ID), It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_NoReconnectClientsIdsStoreContainsClientId_ClearsStore()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = PrepareServerSentEventsNoReconnectClientsIdsStoreMock(); ;
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext();

            await serverSentEventsMiddleware.Invoke(context, null);

            Assert.Equal(StatusCodes.Status204NoContent, context.Response.StatusCode);

            serverSentEventsNoReconnectClientsIdsStoreMock.Verify(o => o.RemoveClientIdAsync(It.Is<Guid>(clientId => clientId == CLIENT_ID)), Times.Once);
        }
        #endregion
    }
}
