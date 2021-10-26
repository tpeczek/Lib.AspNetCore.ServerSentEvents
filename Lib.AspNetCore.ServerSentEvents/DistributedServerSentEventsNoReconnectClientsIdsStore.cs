using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Lib.AspNetCore.ServerSentEvents
{
    internal class DistributedServerSentEventsNoReconnectClientsIdsStore : IServerSentEventsNoReconnectClientsIdsStore
    {
        private readonly IDistributedCache _cache;
        private readonly byte[] _dummyItem = new byte[0];

        public DistributedServerSentEventsNoReconnectClientsIdsStore(IDistributedCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task AddClientId(Guid clientId)
        {
            await _cache.SetAsync(clientId.ToString(), _dummyItem);
        }

        public async Task<bool> ContainsClientId(Guid clientId)
        {
            return (await _cache.GetAsync(clientId.ToString())) is null;
        }

        public async Task RemoveClientId(Guid clientId)
        {
            await _cache.RemoveAsync(clientId.ToString());
        }
    }
}
