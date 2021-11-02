using System;
using System.Threading.Tasks;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class NoOpServerSentEventsNoReconnectClientsIdsStore : IServerSentEventsNoReconnectClientsIdsStore
    {
        public Task AddClientIdAsync(Guid clientId)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ContainsClientIdAsync(Guid clientId)
        {
            return Task.FromResult(false);
        }

        public Task RemoveClientIdAsync(Guid clientId)
        {
            return Task.CompletedTask;
        }
    }
}
