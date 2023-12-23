using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Unit.Middleware.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Unit.Middleware
{
    public class DisconnectTests
    {
        #region Fields
        private static readonly Guid CLIENT_ID = Guid.Parse("90b0401d-9ac7-45bc-841f-04c5b7f9496a");
        #endregion

        #region Prepare SUT
        private class TestHttpRequestLifetimeFeature : IHttpRequestLifetimeFeature
        {
            private readonly CancellationTokenSource _requestAbortedCancellationTokenSource = new CancellationTokenSource();

            public CancellationToken RequestAborted
            {
                get => _requestAbortedCancellationTokenSource.Token;
                set => throw new NotSupportedException();
            }

            public void Abort()
            {
                _requestAbortedCancellationTokenSource.Cancel();
            }
        }

        private Mock<IServerSentEventsClientIdProvider> PrepareServerSentEventsClientIdProviderMock()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = new Mock<IServerSentEventsClientIdProvider>();
            serverSentEventsClientIdProviderMock.Setup(m => m.AcquireClientId(It.IsAny<HttpContext>())).Returns(CLIENT_ID);

            return serverSentEventsClientIdProviderMock;
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Invoke_DisconnectedByClient_DoesNotPreventReconnect()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = new Mock<IServerSentEventsNoReconnectClientsIdsStore>();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object);
            
            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(httpRequestLifetimeFeature: new TestHttpRequestLifetimeFeature());

            Task middlewareInvokeTask = serverSentEventsMiddleware.Invoke(context, null);

            context.Abort();

            await middlewareInvokeTask;

            serverSentEventsNoReconnectClientsIdsStoreMock.Verify(o => o.AddClientIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Invoke_DisconnectedByClient_ReleasesClientId()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(httpRequestLifetimeFeature: new TestHttpRequestLifetimeFeature());

            Task middlewareInvokeTask = serverSentEventsMiddleware.Invoke(context, null);

            context.Abort();

            await middlewareInvokeTask;

            serverSentEventsClientIdProviderMock.Verify(o => o.ReleaseClientId(It.Is<Guid>(clientId => clientId == CLIENT_ID), It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_DisconnectedByServer_PreventsReconnect()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = new Mock<IServerSentEventsNoReconnectClientsIdsStore>();
            ServerSentEventsService serverSentEventsService = new TestServerSentEventsService();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object,
                serverSentEventsService: serverSentEventsService);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(httpRequestLifetimeFeature: new TestHttpRequestLifetimeFeature());

            Task middlewareInvokeTask = serverSentEventsMiddleware.Invoke(context, null);

            await serverSentEventsService.GetClient(CLIENT_ID).DisconnectAsync();

            await middlewareInvokeTask;

            serverSentEventsNoReconnectClientsIdsStoreMock.Verify(o => o.AddClientIdAsync(It.Is<Guid>(clientId => clientId == CLIENT_ID)), Times.Once);
        }

        [Fact]
        public async Task Invoke_DisconnectedByServer_DoesNotReleaseClientId()
        {
            Mock<IServerSentEventsClientIdProvider> serverSentEventsClientIdProviderMock = PrepareServerSentEventsClientIdProviderMock();
            Mock<IServerSentEventsNoReconnectClientsIdsStore> serverSentEventsNoReconnectClientsIdsStoreMock = new Mock<IServerSentEventsNoReconnectClientsIdsStore>();
            ServerSentEventsService serverSentEventsService = new TestServerSentEventsService();
            ServerSentEventsMiddleware<ServerSentEventsService> serverSentEventsMiddleware = SubjectUnderTestHelper.PrepareServerSentEventsMiddleware(
                serverSentEventsClientIdProvider: serverSentEventsClientIdProviderMock.Object,
                serverSentEventsNoReconnectClientsIdsStore: serverSentEventsNoReconnectClientsIdsStoreMock.Object,
                serverSentEventsService: serverSentEventsService);

            HttpContext context = SubjectUnderTestHelper.PrepareHttpContext(httpRequestLifetimeFeature: new TestHttpRequestLifetimeFeature());

            Task middlewareInvokeTask = serverSentEventsMiddleware.Invoke(context, null);

            await serverSentEventsService.GetClient(CLIENT_ID).DisconnectAsync();

            await middlewareInvokeTask;

            serverSentEventsClientIdProviderMock.Verify(o => o.ReleaseClientId(It.IsAny<Guid>(), It.IsAny<HttpContext>()), Times.Never);
        }
        #endregion
    }
}
