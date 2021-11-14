using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Amazon.Runtime.Internal;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace ConventionApiLibrary
{
    public interface ISimpleDbConverter<T>
    {
        T Translate(string itemName, List<Attribute> itemAttributes);
        string GetItemName(T item);
        void PopulateValues(T item, List<ReplaceableAttribute> attributes, List<Attribute> deletes);
        string GetSimpleDbFieldNameFor(Expression<Func<T, object>> dtoField);
        T CreateInstance(string name);
    }

    public class SimpleDbBasedStore<T> where T : class
    {
        private readonly IAmazonSimpleDB _simpleDbClient;
        private readonly string _domainName;
        private readonly ISimpleDbConverter<T> _converter;
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(3);

        public SimpleDbBasedStore(IAmazonSimpleDB simpleDbClient, string domainName, ISimpleDbConverter<T> converter)
        {
            _simpleDbClient = simpleDbClient;
            _domainName = domainName;
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

        public T SelectFromSdbWithSingleResult(string sdbQuery)
        {
            var request = new SelectRequest
            {
                SelectExpression = sdbQuery
            };
            var retrievalTask = _simpleDbClient.SelectAsync(request);

            retrievalTask.Wait(_timeout);

            if (retrievalTask.Result.Items.Count == 0)
            {
                return null;
            }

            if (retrievalTask.Result.Items.Count > 1)
            {
                throw new ArgumentException("More than one match.");
            }

            var item = retrievalTask.Result.Items[0];

            return _converter.Translate(retrievalTask.Result.Items[0].Name, item.Attributes);
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


        private void AddForUpdateOrDelete(string attributeName, string value, List<ReplaceableAttribute> updateList, List<Attribute> deleteList)
        {
            if (value == null)
            {
                deleteList.Add(new Attribute(attributeName, null));
            }
            else
            {
                updateList.Add(new ReplaceableAttribute(attributeName, value, true));
            }
        }

        public IEnumerable<T> SelectItemsBySimpleFilter(Expression<Func<T, object>> dtoField, string filterValue)
        {
            var usernameField = _converter.GetSimpleDbFieldNameFor(dtoField);
            filterValue = filterValue.Replace("'", "''");
            var query = $"SELECT * FROM `{_domainName}` WHERE `{usernameField}` = \"{filterValue}\"";
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
