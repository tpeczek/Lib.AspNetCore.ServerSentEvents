using System;
using System.IO;
using System.Text;
using System.Buffers;
using System.Net.Http;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using Lib.AspNetCore.ServerSentEvents;
using Test.AspNetCore.ServerSentEvents.Functional.Infrastructure;

namespace Test.AspNetCore.ServerSentEvents.Functional
{
    public class KeepAliveTests
    {
        #region Fields
        private const int KEEPALIVE_INTERVAL = 1;
        private readonly static TimeSpan KEEPALIVE_TIMESPAN = TimeSpan.FromSeconds(KEEPALIVE_INTERVAL);

        private const string DEFAULT_KEEPALIVE = ": KEEPALIVE\r\n\r\n";
        #endregion

        #region SUT
        private class KeepaliveModeNeverServerSentEventsServerStartup : FakeServerSentEventsServerStartup
        {
            public KeepaliveModeNeverServerSentEventsServerStartup(IConfiguration configuration) : base(configuration)
            { }

            protected override Action<ServerSentEventsServiceOptions<ServerSentEventsService>> ConfigureServerSentEventsOption
            {
                get
                {
                    return options =>
                    {
                        options.KeepaliveMode = ServerSentEventsKeepaliveMode.Never;
                        options.KeepaliveInterval = KEEPALIVE_INTERVAL;
                    };
                }
            }
        }

        private class KeepaliveModeAlwaysServerSentEventsServerStartup : FakeServerSentEventsServerStartup
        {
            public KeepaliveModeAlwaysServerSentEventsServerStartup(IConfiguration configuration) : base(configuration)
            { }

            protected override Action<ServerSentEventsServiceOptions<ServerSentEventsService>> ConfigureServerSentEventsOption
            {
                get
                {
                    return options =>
                    {
                        options.KeepaliveMode = ServerSentEventsKeepaliveMode.Always;
                        options.KeepaliveInterval = KEEPALIVE_INTERVAL;
                    };
                }
            }
        }
        #endregion

        #region Tests
        [Fact]
        public async Task ServerSentEventsServer_KeepaliveModeNever_DoesNotSendKeepalive()
        {
            using FakeServerSentEventsServerrApplicationFactory<KeepaliveModeNeverServerSentEventsServerStartup> serverSentEventsServerApplicationFactory = new();
            HttpClient serverSentEventsClient = serverSentEventsServerApplicationFactory.CreateClient();

            string serverSentEvents = await GetServerSentEvents(serverSentEventsClient).ConfigureAwait(false);

            Assert.Equal(String.Empty, serverSentEvents);
        }

        [Fact]
        public async Task ServerSentEventsServer_KeepaliveModeAlways_SendsDefaultKeepalive()
        {
            using FakeServerSentEventsServerrApplicationFactory<KeepaliveModeAlwaysServerSentEventsServerStartup> serverSentEventsServerApplicationFactory = new ();
            HttpClient serverSentEventsClient = serverSentEventsServerApplicationFactory.CreateClient();

            string serverSentEvents = await GetServerSentEvents(serverSentEventsClient).ConfigureAwait(false);

            Assert.Matches($"^({DEFAULT_KEEPALIVE})+$", serverSentEvents);
        }

        private static async Task<string> GetServerSentEvents(HttpClient serverSentEventsClient)
        {
            Stopwatch keepaliveStopwatch = new Stopwatch();
            string serverSentEventsResponseContent = String.Empty;

            serverSentEventsClient.DefaultRequestHeaders.Add("Accept", "text/event-stream");
            using (HttpResponseMessage serverSentEventsResponse = await serverSentEventsClient.GetAsync(FakeServerSentEventsServerStartup.SERVER_SENT_EVENTS_ENDPOINT, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                serverSentEventsResponse.EnsureSuccessStatusCode();

                keepaliveStopwatch.Start();

                using (Stream responseStream = await serverSentEventsResponse.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    do
                    {
                        byte[] buffer = ArrayPool<byte>.Shared.Rent(128);

                        try
                        {
                            CancellationTokenSource keepaliveCancellationTokenSource = new CancellationTokenSource();
                            keepaliveCancellationTokenSource.CancelAfter(KEEPALIVE_TIMESPAN);

                            int bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length, keepaliveCancellationTokenSource.Token).ConfigureAwait(false);
                            serverSentEventsResponseContent += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        }
                        catch (OperationCanceledException)
                        { }

                        ArrayPool<byte>.Shared.Return(buffer);
                    } while (keepaliveStopwatch.Elapsed < KEEPALIVE_TIMESPAN);
                }
            }

            keepaliveStopwatch.Stop();

            return serverSentEventsResponseContent;
        }
        #endregion
    }
}
