using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Extensions;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Test.AspNetCore.ServerSentEvents.Extensions
{
    public class ServerSentEventsClientConnectedArgsExtensionsTests
    {
        #region Fields
        private const string HEADER_1_KEY = "header-1";
        private const string HEADER_1_VALUE = "value-1";
        private const string HEADER_2_A_KEY = "header-2a";
        private const string HEADER_2_A_VALUE = "value-2a";
        private const string HEADER_2_B_KEY = "header-2b";
        private const string HEADER_2_B_VALUE = "value-2b";
        private const string HEADER_3_KEY = "header-3";
        private const string HEADER_3_VALUE = "value-3";
        #endregion

        #region Prepare SUT
        private static ServerSentEventsClientConnectedArgs PrepareServerSentEventsClientConnectedArgs(IDictionary<string, StringValues> headers, IDictionary<string, string> properties)
        {
            return new ServerSentEventsClientConnectedArgs(
                PrepareHttpRequest(headers),
                PrepareClient(properties)
                );
        }

        private static HttpRequest PrepareHttpRequest(IDictionary<string, StringValues> headers)
        {
            var context = new DefaultHttpContext();

            foreach (var header in headers)
            {
                context.Request.Headers.Add(header);
            }

            return context.Request;
        }

        private static IServerSentEventsClient PrepareClient(IDictionary<string, string> properties)
        {
            var context = new DefaultHttpContext();

            var client = new ServerSentEventsClient(Guid.Empty, new ClaimsPrincipal(), context.Response);

            foreach (var property in properties)
            {
                client.Properties.TryAdd(property.Key, property.Value);
            }

            return client;
        }
        #endregion

        #region Tests
        [Fact]
        public void AddPropertyToClientFromHeader_ShouldAddProperty_WhenItDoesNotExist()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_2_A_KEY, HEADER_2_A_VALUE
                              },
                              {
                                  HEADER_2_B_KEY, HEADER_2_B_VALUE
                              },
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       }
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertyToClientFromHeader(HEADER_2_A_KEY);

            //ASSERT
            Assert.Contains(new KeyValuePair<string,string>(HEADER_2_A_KEY, HEADER_2_A_VALUE), args.Client.Properties);
        }

        [Fact]
        public void AddPropertyToClientFromHeader_ShouldUpdateProperty_WhenInstructedToOverwrite()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_1_KEY, HEADER_2_A_VALUE
                              }
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       }
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertyToClientFromHeader(HEADER_1_KEY, true);

            //ASSERT
            Assert.Contains(new KeyValuePair<string, string>(HEADER_1_KEY, HEADER_2_A_VALUE), args.Client.Properties);
        }

        [Fact]
        public void AddPropertyToClientFromHeader_ShouldNotUpdateProperty_WhenNotInstructedToOverwrite()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_1_KEY, HEADER_2_A_VALUE
                              }
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       }
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertyToClientFromHeader(HEADER_1_KEY, false);

            //ASSERT
            Assert.Contains(new KeyValuePair<string, string>(HEADER_1_KEY, HEADER_1_VALUE), args.Client.Properties);
        }

        [Fact]
        public void AddPropertiesToClientFromHeaders_ShouldAddProperty_WhenItDoesNotExist()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_2_A_KEY, HEADER_2_A_VALUE
                              },
                              {
                                  HEADER_2_B_KEY, HEADER_2_B_VALUE
                              },
                              {
                                  HEADER_3_KEY, HEADER_3_VALUE
                              },
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       }
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertiesToClientFromHeaders(new Regex(@"header-2\.*"));

            //ASSERT
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_A_KEY, HEADER_2_A_VALUE), args.Client.Properties);
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_B_KEY, HEADER_2_B_VALUE), args.Client.Properties);
            Assert.DoesNotContain(new KeyValuePair<string, string>(HEADER_3_KEY, HEADER_3_VALUE), args.Client.Properties);
        }

        [Fact]
        public void AddPropertiesToClientFromHeaders_ShouldUpdateProperty_WhenInstructedToOverwrite()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_2_A_KEY, HEADER_2_A_VALUE
                              },
                              {
                                  HEADER_2_B_KEY, HEADER_2_B_VALUE
                              },
                              {
                                  HEADER_3_KEY, HEADER_3_VALUE
                              },
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       },
                                       {
                                           HEADER_2_A_KEY, HEADER_2_B_VALUE
                                       },
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertiesToClientFromHeaders(new Regex(@"header-2\.*"), true);

            //ASSERT
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_A_KEY, HEADER_2_A_VALUE), args.Client.Properties);
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_B_KEY, HEADER_2_B_VALUE), args.Client.Properties);
            Assert.DoesNotContain(new KeyValuePair<string, string>(HEADER_3_KEY, HEADER_3_VALUE), args.Client.Properties);
        }

        [Fact]
        public void AddPropertiesToClientFromHeaders_ShouldNotUpdateProperty_WhenNotInstructedToOverwrite()
        {
            // ARRANGE
            var headers = new Dictionary<string, StringValues>
                          {
                              {
                                  HEADER_2_A_KEY, HEADER_2_A_VALUE
                              },
                              {
                                  HEADER_2_B_KEY, HEADER_2_B_VALUE
                              },
                              {
                                  HEADER_3_KEY, HEADER_3_VALUE
                              },
                          };

            var clientProperties = new Dictionary<string, string>
                                   {
                                       {
                                           HEADER_1_KEY, HEADER_1_VALUE
                                       },
                                       {
                                           HEADER_2_A_KEY, HEADER_2_B_VALUE
                                       },
                                   };

            var args = PrepareServerSentEventsClientConnectedArgs(headers, clientProperties);

            // ACT
            args.AddPropertiesToClientFromHeaders(new Regex(@"header-2\.*"), false);

            //ASSERT
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_A_KEY, HEADER_2_B_VALUE), args.Client.Properties);
            Assert.Contains(new KeyValuePair<string, string>(HEADER_2_B_KEY, HEADER_2_B_VALUE), args.Client.Properties);
            Assert.DoesNotContain(new KeyValuePair<string, string>(HEADER_3_KEY, HEADER_3_VALUE), args.Client.Properties);
        }

        #endregion
    }
}
