using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            List<(Expression<Func<PersistedGrant, object>>, string)> conditions = new List<(Expression<Func<PersistedGrant, object>>, string)>();

            if (!string.IsNullOrEmpty(filter.ClientId))
            {
                conditions.Add((pg => pg.ClientId, filter.ClientId));
            }
            if (!string.IsNullOrEmpty(filter.SessionId))
            {
                conditions.Add((pg => pg.SessionId, filter.SessionId));
            }
            if (!string.IsNullOrEmpty(filter.SubjectId))
            {
                conditions.Add((pg => pg.SubjectId, filter.SubjectId));
            }
            if (!string.IsNullOrEmpty(filter.Type))
            {
                conditions.Add((pg => pg.Type, filter.Type));
            }

            if (conditions.Count == 0)
            {
                throw new ArgumentException("No conditions in filter. Expecting at least one of ClientId, SessionId, SubjectId and Type to be set.");
            }

            var where = _grantStore.Where(conditions[0].Item1, conditions[0].Item2);
            for(var i = 1; i < conditions.Count; i++)
            {
                where = where.AndAlso(conditions[i].Item1, conditions[i].Item2);
            }

            return Task.FromResult(where.Select());
        }

        public Task RemoveAsync(string key)
        {
            return _grantStore.DeleteByItemName(key);
        }

        public Task RemoveAllAsync(PersistedGrantFilter filter)
        {
            var grantKeys = GetAllAsync(filter);
            grantKeys.Wait();

            var deleteTasks = grantKeys
                .Result
                .Select(x => x.Key)
                .Select(x => _grantStore.DeleteByItemName(x))
                .ToArray();

            return Task.WhenAll(deleteTasks);
        }
    }
}