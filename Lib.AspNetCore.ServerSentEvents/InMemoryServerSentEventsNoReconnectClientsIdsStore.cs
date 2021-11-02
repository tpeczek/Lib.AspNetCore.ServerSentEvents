using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class InMemoryServerSentEventsNoReconnectClientsIdsStore : IServerSentEventsNoReconnectClientsIdsStore
    {
        private readonly ConcurrentDictionary<Guid, bool> _store = new ConcurrentDictionary<Guid, bool>();

        public Task AddClientIdAsync(Guid clientId)
        {
            _store.TryAdd(clientId, true);

            return Task.CompletedTask;
        }

        public Task<bool> ContainsClientIdAsync(Guid clientId)
        {
            return Task.FromResult(_store.ContainsKey(clientId));
        }

        public Task RemoveClientIdAsync(Guid clientId)
        {
            _store.TryRemove(clientId, out _);

            return Task.CompletedTask;
        }
    }
}
