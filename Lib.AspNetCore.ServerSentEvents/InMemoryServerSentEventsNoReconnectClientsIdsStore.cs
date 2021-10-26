using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class InMemoryServerSentEventsNoReconnectClientsIdsStore : IServerSentEventsNoReconnectClientsIdsStore
    {
        private readonly ConcurrentDictionary<Guid, bool> _store = new ConcurrentDictionary<Guid, bool>();

        public Task AddClientId(Guid clientId)
        {
            _store.TryAdd(clientId, true);

            return Task.CompletedTask;
        }

        public Task<bool> ContainsClientId(Guid clientId)
        {
            return Task.FromResult(_store.ContainsKey(clientId));
        }

        public Task RemoveClientId(Guid clientId)
        {
            _store.TryRemove(clientId, out _);

            return Task.CompletedTask;
        }
    }
}
