using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace ConventionApiLibrary.DataAccess
{
    public class SimpleDbBasedStore<T> where T : class
    {
        private readonly IAmazonSimpleDB _simpleDbClient;
        private readonly string _domainName;
        private readonly ISimpleDbConverter<T> _converter;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);

        public SimpleDbBasedStore(IAmazonSimpleDB simpleDbClient, ISimpleDbDomainName domainName, ISimpleDbConverter<T> converter)
        {
            _simpleDbClient = simpleDbClient;
            _domainName = domainName.DomainName;
            _converter = converter;
        }

        public T FindByItemName(string itemName)
        {
            var request = new GetAttributesRequest(_domainName, itemName);
            var item = _simpleDbClient.GetAttributesAsync(request);

            item.Wait();

            if (!item.Result.Attributes.Any())
            {
                return null;
            }

            return _converter.Translate(itemName, item.Result.Attributes);
        }

        public async Task DeleteByItemName(string subjectId)
        {
            var request = new GetAttributesRequest(_domainName, subjectId);
            var retrievalTask = await _simpleDbClient.GetAttributesAsync(request);

            var deleteRequest = new DeleteAttributesRequest(_domainName, subjectId)
            {
                Attributes = retrievalTask.Attributes
            };

            await _simpleDbClient.DeleteAttributesAsync(deleteRequest);
        }

        public async Task Store(T item)
        {
            string itemName = _converter.GetItemName(item);
            if (itemName == null)
            {
                throw new ArgumentException(nameof(item), "Cannot store users without a subjectId");
            }
            // Test if user exists. This is of course not transactional/multi thread safe.

            var deletes = new List<Attribute>();
            var attributes = new List<ReplaceableAttribute>();

            _converter.PopulateValues(item, attributes, deletes);

            await StoreInSimpleDb(itemName, attributes, deletes);

        }
        private async Task StoreInSimpleDb(string subjectId, List<ReplaceableAttribute> attributes, List<Attribute> deletes)
        {
            if (attributes.Count > 0)
            {
                var putRequest = new PutAttributesRequest(_domainName, subjectId, attributes);
                await _simpleDbClient.PutAttributesAsync(putRequest);
            }

            if (deletes.Count > 0)
            {
                var deleteRequest = new DeleteAttributesRequest(_domainName, subjectId, deletes);
                await _simpleDbClient.DeleteAttributesAsync(deleteRequest);
            }
        }

        public FilterBuilder Where(Expression<Func<T, object>> dtoField, string filterValue)
        {
            return new FilterBuilder(dtoField, filterValue, _timeout, _converter, _domainName, _simpleDbClient);
        }

        public class FilterBuilder
        {
            private readonly Expression<Func<T, object>> _dtoField;
            private readonly string _filterValue;
            private readonly TimeSpan _timeout;
            private readonly ISimpleDbConverter<T> _converter;
            private readonly string _domainName;
            private readonly IAmazonSimpleDB _simpleDbClient;
            private readonly FilterBuilder _parent;
            private readonly string _dtoFieldNameInSimpleDb;

            public FilterBuilder(Expression<Func<T, object>> dtoField, string filterValue, TimeSpan timeout, ISimpleDbConverter<T> converter, string domainName, IAmazonSimpleDB simpleDbClient,
                FilterBuilder parent = null)
            {
                _dtoField = dtoField;
                _filterValue = filterValue.Replace("'", "''");
                _timeout = timeout;
                _converter = converter;
                _domainName = domainName;
                _simpleDbClient = simpleDbClient;
                _parent = parent;
                _dtoFieldNameInSimpleDb = _converter.GetSimpleDbFieldNameFor(_dtoField);
            }

            public FilterBuilder AndAlso(Expression<Func<T, object>> dtoField, string filterValue)
            {
                return new FilterBuilder(dtoField, filterValue, _timeout, _converter, _domainName, _simpleDbClient, this);
            }

            public IEnumerable<T> Select()
            {
                var query = $"SELECT * FROM `{_domainName}` WHERE `{_dtoFieldNameInSimpleDb}` = \"{_filterValue}\"";
                var current = this;
                while (current._parent != null)
                {
                    query += $" AND `{_parent._dtoFieldNameInSimpleDb}` = \"{_parent._filterValue}\"";
                    current = current._parent;
                }
                var request = new SelectRequest(query, false);

                do
                {
                    var result = _simpleDbClient.SelectAsync(request);
                    result.Wait(_timeout);

                    foreach (var item in result.Result.Items)
                    {
                        yield return _converter.Translate(item.Name, item.Attributes);
                    }

                    request.NextToken = result.Result.NextToken;
                } while (request.NextToken != null);
            }
        }
    }
}
