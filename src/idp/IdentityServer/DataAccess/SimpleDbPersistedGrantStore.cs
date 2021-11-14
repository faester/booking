using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConventionApiLibrary.DataAccess;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer.DataAccess
{
    public class SimpleDbPersistedGrantStore : IPersistedGrantStore
    {
        private SimpleDbBasedStore<PersistedGrant> _grantStore;

        public SimpleDbPersistedGrantStore(SimpleDbBasedStore<PersistedGrant> grantStore)
        {
            _grantStore = grantStore;
        }

        public Task StoreAsync(PersistedGrant grant)
        {
            return _grantStore.Store(grant);
        }

        public Task<PersistedGrant> GetAsync(string key)
        {
            return Task.FromResult(_grantStore.FindByItemName(key));
        }

        public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string key)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}