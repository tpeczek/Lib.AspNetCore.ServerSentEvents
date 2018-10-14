using System;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Benchmark.AspNetCore.ServerSentEvents.Infrastructure;

namespace Benchmark.AspNetCore.ServerSentEvents.Benchmarks
{
    [MemoryDiagnoser]
    public class ServerSentEventsServiceBenchmarks
    {
        #region Fields
        private const int MULTIPLE_CLIENTS_COUNT = 10000;

        private const string EVENT_TYPE = "Benchmark";
        private const string EVENT_DATA = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private readonly ServerSentEventsClient _serverSentEventsClient;
        private readonly ServerSentEventsService _serverSentEventsService;
        private readonly ServerSentEvent _event = new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = EVENT_TYPE,
            Data = new List<string> { EVENT_DATA }
        };
        #endregion

        #region Constructor
        public ServerSentEventsServiceBenchmarks()
        {
            _serverSentEventsClient = new ServerSentEventsClient(Guid.NewGuid(), new ClaimsPrincipal(), new NoOpHttpResponse());

            _serverSentEventsService = new ServerSentEventsService();
            for (int i = 0; i < MULTIPLE_CLIENTS_COUNT; i++)
            {
                _serverSentEventsService.AddClient(new ServerSentEventsClient(Guid.NewGuid(), new ClaimsPrincipal(), new NoOpHttpResponse()));
            }
        }
        #endregion

        #region Benchmarks
        [Benchmark]
        public Task SendEventAsync_SingleData_SingleClient()
        {
            return _serverSentEventsClient.SendEventAsync(EVENT_DATA);
        }

        [Benchmark]
        public Task SendEventAsync_SingleEvent_SingleClient()
        {
            return _serverSentEventsClient.SendEventAsync(_event);
        }

        [Benchmark]
        public Task ChangeReconnectIntervalAsync_SingleClient()
        {
            return _serverSentEventsClient.ChangeReconnectIntervalAsync(5000, CancellationToken.None);
        }

        [Benchmark]
        public Task SendEventAsync_SingleData_MultipleClients()
        {
            return _serverSentEventsService.SendEventAsync(EVENT_DATA);
        }

        [Benchmark]
        public Task SendEventAsync_SingleEvent_MultipleClients()
        {
            return _serverSentEventsService.SendEventAsync(_event);
        }

        [Benchmark]
        public Task ChangeReconnectIntervalAsync_MultipleClients()
        {
            return _serverSentEventsService.ChangeReconnectIntervalAsync(5000);
        }
        #endregion
    }
}
