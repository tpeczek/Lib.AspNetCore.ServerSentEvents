using System;
using System.Security.Claims;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Test.AspNetCore.ServerSentEvents
{
    public class ServerSentEventsClientTests
    {
        #region Fields
        private const string PROPERTY_1_NAME = "property-1";
        private const string PROPERTY_1_VALUE = "property-1-value";
        private const string PROPERTY_1_UPDATED_VALUE = "property-1-updated-value";
        #endregion

        #region Prepare SUT
        private static ServerSentEventsClient PrepareServerSentEventsClient()
        {
            var context = new DefaultHttpContext();
            return new ServerSentEventsClient(Guid.NewGuid(), new ClaimsPrincipal(), context.Response, false);
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
            var expected = client.GetProperty<string>(PROPERTY_1_NAME);

            // ASSERT
            Assert.Equal(expected, PROPERTY_1_VALUE);
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
        #endregion
    }
}
