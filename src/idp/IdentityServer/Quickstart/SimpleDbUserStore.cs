using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Test;
using IdentityServerHost.Quickstart.UI;
using Newtonsoft.Json;
using Attribute = Amazon.SimpleDB.Model.Attribute;

namespace IdentityServer.Quickstart
{
    public class SimpleDbUserStore : IUserStore
    {
        private const string USERNAME_FIELD = "username";
        private const string IS_ACTIVE = "isActive";
        private const string CLAIMS_FIELD = "claims";
        private const string PASSWORD_FIELD = "pwd";
        private const string PROVIDERNAME_FIELD = "provider";
        private const string PROVIDER_SUBJECTID_FIELD = "provider_subject";
        private const string ADDRESS_FIELD = "address";
        private const string PHONE_FIELD = "phone";
        private const string EMAIL_FIELD = "email";
        private readonly IAmazonSimpleDB _simpleDbClient;
        private readonly IPasswordFunctions _passwordFunctions;
        private readonly string _domainName;
        private static TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public SimpleDbUserStore(IAmazonSimpleDB simpleDbClient, IPasswordFunctions passwordFunctions, string domainName = "users")
        {
            _simpleDbClient = simpleDbClient;
            _passwordFunctions = passwordFunctions;
            _domainName = domainName;
        }

        public TestUser ValidateCredentials(string username, string password)
        {
            var user = FindByUsername(username);
            if (user == null)
            {
                return null;
            }

            if (user.Password == null)
            {
                return null;
            }

            if (!_passwordFunctions.IsHashedWithThisFunction(user.Password))
            {
                throw new ArgumentException("The users password is hashed with a wrong algorithm.");
            }

            return _passwordFunctions.CompareHashedPassword(password, user.Password)
                ? user
                : null;

        }

        public TestUser FindBySubjectId(string subjectId)
        {
            if (!Guid.TryParse(subjectId, out var subjectIdGuid))
            {
                return null;
            }

            var request = new GetAttributesRequest(_domainName, subjectId);
            var item =_simpleDbClient.GetAttributesAsync(request);

            item.Wait();

            if (!item.Result.Attributes.Any())
            {
                return null;
            }

            return Translate(subjectIdGuid, item.Result.Attributes);
        }

        private TestUser Translate(Guid userid, List<Attribute> itemResult)
        {
            var result = new TestUser();
            result.SubjectId = userid.ToString();
            foreach(var attribute in itemResult)
            {
                switch (attribute.Name)
                {
                    case USERNAME_FIELD:
                        result.Username = attribute.Value;
                        break;
                    case IS_ACTIVE:
                        result.IsActive = attribute.Value == "true";
                        break;
                    case PASSWORD_FIELD:
                        result.Password = attribute.Value;
                        break;
                    case CLAIMS_FIELD:
                        result.Claims = JsonConvert.DeserializeObject<List<Claim>>(attribute.Value);
                        break;
                    case PROVIDERNAME_FIELD:
                        result.ProviderName = attribute.Value;
                        break;
                    case PROVIDER_SUBJECTID_FIELD:
                        result.ProviderSubjectId = attribute.Value;
                        break;
                }
            }

            return result;
        }

        public TestUser FindByUsername(string username)
        {
            username = CanonicalizeUsername(username);
            var sdbQuery = $"SELECT * FROM `{_domainName}` WHERE {USERNAME_FIELD} = '{username}'";
            return SelectFromSdbWithSingleResult(sdbQuery);
        }

        private TestUser SelectFromSdbWithSingleResult(string sdbQuery)
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

            return Translate(Guid.Parse(retrievalTask.Result.Items[0].Name), item.Attributes);
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

        public async Task Store(TestUser user)
        {
            if (user.SubjectId == null)
            {
                throw new ArgumentException(nameof(user.SubjectId), "Cannot store users without a SubjectId");
            }
            // Test if user exists. This is of course not transactional/multi thread safe.
            var existing = FindByUsername(user.Username);
            if (existing != null && existing.SubjectId != user.SubjectId)
            {
                throw new ArgumentException("Duplicate users");
            }

            var deletes = new List<Attribute>();
            var attributes = new List<ReplaceableAttribute>();
            AddForUpdateOrDelete(IS_ACTIVE, user.IsActive ? "true" : "false", attributes, deletes);
            AddForUpdateOrDelete(USERNAME_FIELD, CanonicalizeUsername(user.Username), attributes, deletes);
            AddForUpdateOrDelete(PASSWORD_FIELD, user.Password == null ? null : (_passwordFunctions.CreateHash(user.Password)), attributes, deletes);
            AddForUpdateOrDelete(PROVIDERNAME_FIELD, user.ProviderName, attributes, deletes);
            AddForUpdateOrDelete(PROVIDER_SUBJECTID_FIELD, user.ProviderSubjectId, attributes, deletes);
            AddForUpdateOrDelete(CLAIMS_FIELD,  user.Claims == null ? null : user.ProviderSubjectId, attributes, deletes);

            await StoreInSimpleDb(user.SubjectId, attributes, deletes);
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

        public async Task StoreUserInformation(string subjectId, UserInformation userInfo)
        {
            var deletes = new List<Attribute>();
            var attributes = new List<ReplaceableAttribute>();
            AddForUpdateOrDelete(PHONE_FIELD, userInfo.Phone, attributes, deletes);
            AddForUpdateOrDelete(ADDRESS_FIELD, userInfo.Address, attributes, deletes);
            AddForUpdateOrDelete(EMAIL_FIELD, userInfo.Email, attributes, deletes);

            await StoreInSimpleDb(subjectId, attributes, deletes);
        }

        public async Task<UserInformation> GetUserInformation(string subjectId)
        {
            var request = new SelectRequest
            {
                SelectExpression = $"SELECT {ADDRESS_FIELD}, {PHONE_FIELD}, {EMAIL_FIELD} " +
                                   $"FROM `{_domainName}` " +
                                   $"WHERE itemName() = '{subjectId}'",
            };
            var retrievalTask = await _simpleDbClient.SelectAsync(request);

            if (retrievalTask.Items.Count == 0)
            {
                return null;
            }

            if (retrievalTask.Items.Count > 1)
            {
                throw new ArgumentException("More than one match.");
            }

            var attributes = retrievalTask.Items[0].Attributes;
            var result = new UserInformation
            {
                Address = attributes.SingleOrDefault(attr => attr.Name == ADDRESS_FIELD)? .Value,
                Phone = attributes.SingleOrDefault(attr => attr.Name == PHONE_FIELD)?.Value,
                Email = attributes.SingleOrDefault(attr => attr.Name == EMAIL_FIELD)?.Value,
            };

            return result;
        }

        public async Task DeleteBySubjectId(string subjectId)
        {
            var request = new GetAttributesRequest(_domainName, subjectId);
            var retrievalTask = await _simpleDbClient.GetAttributesAsync(request);


            var deleteRequest = new DeleteAttributesRequest(_domainName, subjectId)
            {
                Attributes = retrievalTask.Attributes
            };

            await _simpleDbClient.DeleteAttributesAsync(deleteRequest);
        }

        private static string CanonicalizeUsername(string username)
        {
            return username.ToLower(CultureInfo.InvariantCulture);
        }
    }
}
