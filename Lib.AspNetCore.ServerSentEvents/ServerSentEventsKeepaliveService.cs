using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Lib.AspNetCore.ServerSentEvents.Internals;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class ServerSentEventsKeepaliveService<TServerSentEventsService> : IHostedService, IDisposable
        where TServerSentEventsService : ServerSentEventsService
    {
        #region Fields
        private readonly bool _isBehindAncm = IsBehindAncm();
        private readonly static ServerSentEventBytes _keepaliveServerSentEventBytes = ServerSentEventsHelper.GetCommentBytes("KEEPALIVE");

        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        private readonly ServerSentEventsServiceOptions<TServerSentEventsService> _options;
        private readonly TServerSentEventsService _serverSentEventsService;

        private Task _executingTask;
        #endregion

        #region Constructor
        public ServerSentEventsKeepaliveService(TServerSentEventsService serverSentEventsService, IOptions<ServerSentEventsServiceOptions<TServerSentEventsService>> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _serverSentEventsService = serverSentEventsService;
        }
        #endregion

        #region Methods
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if ((_options.KeepaliveMode == ServerSentEventsKeepaliveMode.Always) || ((_options.KeepaliveMode == ServerSentEventsKeepaliveMode.BehindAncm) && _isBehindAncm))
            {
                _executingTask = ExecuteAsync(_stoppingCts.Token);

                if (_executingTask.IsCompleted)
                {
                    return _executingTask;
                }
            }

            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _serverSentEventsService.SendEventAsync(_keepaliveServerSentEventBytes, CancellationToken.None);

                await Task.Delay(TimeSpan.FromSeconds(_options.KeepaliveInterval), stoppingToken);
            }
        }

        private static bool IsBehindAncm()
        {
            return !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_PORT"))
                && !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_APPL_PATH"))
                && !String.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_TOKEN"));
        }

        public virtual void Dispose()
        {
            _stoppingCts.Cancel();
        }
        #endregion
    }
}
