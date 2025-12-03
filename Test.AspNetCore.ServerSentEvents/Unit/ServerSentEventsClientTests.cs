using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Internals;

namespace Test.AspNetCore.ServerSentEvents.Unit
{
    using System.Threading.Tasks;

    public class ServerSentEventsClientTests
    {
        #region Fields
        private const string PROPERTY_1_NAME = "property-1";
        private const string PROPERTY_1_VALUE = "property-1-value";
        private const string PROPERTY_1_UPDATED_VALUE = "property-1-updated-value";
        #endregion

        #region Prepare SUT
        private static ServerSentEventsClient PrepareServerSentEventsClient(HttpContext context = null, bool clientDisconnectServicesAvailable = false)
        {
            context = context ?? new DefaultHttpContext();
            return new ServerSentEventsClient(Guid.NewGuid(), context, clientDisconnectServicesAvailable);
        }
        #endregion

        #region Tests
        [Fact]
        public void GetProperty_ReturnsTheStoredValue()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE);

            // ACT
            var actual = client.GetProperty<string>(PROPERTY_1_NAME);

            // ASSERT
            Assert.Equal(PROPERTY_1_VALUE, actual);
        }

        [Fact]
        public void GetProperty_ReturnsDefaultIfPropertyNotPresent()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE);

            // ACT
            var actual = client.GetProperty<string>(PROPERTY_1_UPDATED_VALUE);

            // ASSERT
            Assert.Equal(default, actual);
        }

        [Fact]
        public void RemoveProperty_RemovesTheProperty()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE);

            // ACT
            var actual = client.RemoveProperty<string>(PROPERTY_1_NAME);

            // ASSERT
            Assert.Equal(PROPERTY_1_VALUE, actual);
        }

        [Fact]
        public void RemoveProperty_DoesNotBreakIfPropertyDoesNotExist()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            // ACT
            var actual = client.RemoveProperty<string>(PROPERTY_1_NAME);

            // ASSERT
            Assert.Equal(default, actual);
        }

        [Fact]
        public void SetProperty_AddProperty()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            // ACT
            var actual = client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE, true);

            // ASSERT
            var actualValue = client.GetProperty<string>(PROPERTY_1_NAME);

            Assert.True(actual);
            Assert.Equal(PROPERTY_1_VALUE, actualValue);
        }

        [Fact]
        public void SetProperty_UpdateProperty()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE);

            // ACT
            var actual = client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_UPDATED_VALUE, true);

            // ASSERT
            var actualValue = client.GetProperty<string>(PROPERTY_1_NAME);

            Assert.True(actual);
            Assert.Equal(PROPERTY_1_UPDATED_VALUE, actualValue);
        }

        [Fact]
        public void SetProperty_DoesNotUpdateProperty()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_VALUE);

            // ACT
            var actual = client.SetProperty(PROPERTY_1_NAME, PROPERTY_1_UPDATED_VALUE);

            // ASSERT
            var actualValue = client.GetProperty<string>(PROPERTY_1_NAME);

            Assert.False(actual);
            Assert.Equal(PROPERTY_1_VALUE, actualValue);
        }

        [Fact]
        public async Task Disconnect_ClientDisconnectServicesNotAvailable_ThrowsInvalidOperationException()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient();

            // ASSERT
            InvalidOperationException disconnectException = await Assert.ThrowsAsync<InvalidOperationException>(async () => await client.DisconnectAsync());
            Assert.Equal($"Disconnecting a {nameof(ServerSentEventsClient)} requires registering implementations of {nameof(IServerSentEventsClientIdProvider)} and {nameof(IServerSentEventsNoReconnectClientsIdsStore)}.", disconnectException.Message);
        }

        [Fact]
        public async Task Disconnect_ClientDisconnectServicesAvailable_PreventsReconnect()
        {
            // ARRANGE
            var client = PrepareServerSentEventsClient(clientDisconnectServicesAvailable: true);

            // ACT
            await client.DisconnectAsync();

            // ASSERT
            Assert.True(client.PreventReconnect);
        }

        [Fact]
        public async Task Disconnect_ClientDisconnectServicesAvailable_Disconnects()
        {
            // ARRANGE
            HttpContext context = new DefaultHttpContext();

#if NET462
            Mock<IHttpRequestLifetimeFeature> httpRequestLifetimeFeatureMock = new Mock<IHttpRequestLifetimeFeature>();
            context.Features.Set(httpRequestLifetimeFeatureMock.Object);
#else
            Mock<IHttpResponseBodyFeature> httpResponseBodyFeatureMock = new Mock<IHttpResponseBodyFeature>();
            context.Features.Set(httpResponseBodyFeatureMock.Object);
#endif

            var client = PrepareServerSentEventsClient(context: context, clientDisconnectServicesAvailable: true);

            // ACT
            await client.DisconnectAsync();

            // ASSERT
            Assert.False(client.IsConnected);
            Assert.True(client.DisconnectAsync().IsCompleted);

#if NET462
            httpRequestLifetimeFeatureMock.Verify(o => o.Abort(), Times.Once);
#else
            httpResponseBodyFeatureMock.Verify(o => o.CompleteAsync(), Times.Once);
#endif
        }
        #endregion
    }
}
