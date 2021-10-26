using System;
using System.Threading.Tasks;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class NoOpServerSentEventsNoReconnectClientsIdsStore : IServerSentEventsNoReconnectClientsIdsStore
    {
        public Task AddClientId(Guid clientId)
        {
            return Task.CompletedTask;
        }

        public Task<bool> ContainsClientId(Guid clientId)
        {
            return Task.FromResult(false);
        }

        public Task RemoveClientId(Guid clientId)
        {
            return Task.CompletedTask;
        }
    }
}
