using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Lib.AspNetCore.ServerSentEvents;
using Lib.AspNetCore.ServerSentEvents.Internals;
using Benchmark.AspNetCore.ServerSentEvents.Infrastructure;

namespace Benchmark.AspNetCore.ServerSentEvents.Benchmarks
{
    public class ServerSentEventsServiceBenchmarks
    {
        #region Fields
        private const int MULTIPLE_CLIENTS_COUNT = 10000;

        private readonly ServerSentEventsService _serverSentEventsService;
        private readonly ServerSentEvent _event = new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "Benchmark",
            Data = new List<string> { "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum." }
        };
        #endregion

        #region Constructor
        public ServerSentEventsServiceBenchmarks()
        {
            _serverSentEventsService = new ServerSentEventsService();
            for (int i = 0; i < MULTIPLE_CLIENTS_COUNT; i++)
            {
                _serverSentEventsService.AddClient(new ServerSentEventsClient(Guid.NewGuid(), new ClaimsPrincipal(), new NoOpHttpResponse()));
            }
        }
        #endregion

        #region Benchmarks
        [Benchmark]
        public Task SendEventAsync()
        {
            return _serverSentEventsService.SendEventAsync(_event);
        }

        [Benchmark]
        public Task ChangeReconnectIntervalAsync()
        {
            return _serverSentEventsService.ChangeReconnectIntervalAsync(5000);
        }
        #endregion
    }
}
