using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using IdentityServer4.Test;
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
        private const string HASHED_PASSWORD_PREFIX = "BCRYPT:";
        private const string SharedSalt = "L/8V2c+mjflmZdnAC1YG/sYmkV5BA2Zjl7j8M5";
        private readonly IAmazonSimpleDB _simpleDbClient;
        private readonly string _domainName;
        private static TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public SimpleDbUserStore(IAmazonSimpleDB simpleDbClient, string domainName = "users")
        {
            _simpleDbClient = simpleDbClient;
            _domainName = domainName;
        }

        public bool ValidateCredentials(string username, string password)
        {
            throw new System.NotImplementedException();
        }

        public TestUser FindBySubjectId(string subjectId)
        {
            var request = new GetAttributesRequest(_domainName, subjectId);
            var item =_simpleDbClient.GetAttributesAsync(request);

            item.Wait();

            return Translate(item.Result.Attributes);
        }

        private TestUser Translate(List<Attribute> itemResult)
        {
            TestUser result = new TestUser();

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

            return Translate(item.Attributes);
        }

        public TestUser FindByExternalProvider(string provider, string userId)
        {
            throw new System.NotImplementedException();
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
            var deletes = new List<Attribute>();
            var attributes = new List<ReplaceableAttribute>();
            AddForUpdateOrDelete(IS_ACTIVE, user.IsActive ? "true" : "false", attributes, deletes);
            AddForUpdateOrDelete(USERNAME_FIELD, CanonicalizeUsername(user.Username), attributes, deletes);
            AddForUpdateOrDelete(PASSWORD_FIELD, HashPassword(user.Password), attributes, deletes);
            AddForUpdateOrDelete(PROVIDERNAME_FIELD, user.ProviderName, attributes, deletes);
            AddForUpdateOrDelete(PROVIDER_SUBJECTID_FIELD, user.ProviderSubjectId, attributes, deletes);
            AddForUpdateOrDelete(CLAIMS_FIELD,  user.Claims == null ? null : user.ProviderSubjectId, attributes, deletes);

            if (attributes.Count > 0)
            {
                var putRequest = new PutAttributesRequest(_domainName, user.SubjectId, attributes);
                await _simpleDbClient.PutAttributesAsync(putRequest);
            }

            if (deletes.Count > 0)
            {
                var deleteRequest = new DeleteAttributesRequest(_domainName, user.SubjectId, deletes);
                await _simpleDbClient.DeleteAttributesAsync(deleteRequest);
            }
        }

        private string HashPassword(string userPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(userPassword);
        }

        public async Task StorePassword(string subjectId, string password)
        {
            string passwordValueForDb;
            if (password.StartsWith(HASHED_PASSWORD_PREFIX))
            {
                passwordValueForDb = password;
            }
            else
            {
                passwordValueForDb = HASHED_PASSWORD_PREFIX + BCrypt.Net.BCrypt.HashPassword(password, SharedSalt);
            }
            var attributes = new List<ReplaceableAttribute>();
            attributes.Add(new ReplaceableAttribute(PASSWORD_FIELD, passwordValueForDb, true));
            await _simpleDbClient.PutAttributesAsync(new PutAttributesRequest(_domainName, subjectId, attributes));
        }

        

        public Task DeleteBySubjectId(string subjectId)
        {
            throw new System.NotImplementedException();
        }

        private static string CanonicalizeUsername(string username)
        {
            return username.ToLower(CultureInfo.InvariantCulture);
        }

        private class UserDTO
        {

        }
    }
}
