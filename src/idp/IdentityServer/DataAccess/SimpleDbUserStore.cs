using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ConventionApiLibrary.DataAccess;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Test;

namespace IdentityServer.DataAccess
{
    public class SimpleDbUserStore : IUserStore
    {
        private readonly IPasswordFunctions _passwordFunctions;
        private readonly SimpleDbBasedStore<TestUserDto> _simpleDbStore;
        private readonly SimpleDbBasedStore<UserInformation> _simpleDbBasedStoreUserInfo;

        public class TestUserDto
        {
            public static TestUserDto From(TestUser testUser, string valueForPassword)
            {
                var result = new TestUserDto();
                result.SubjectId = testUser.SubjectId;
                result.ProviderSubjectId = testUser.ProviderSubjectId;
                result.Password = valueForPassword;
                result.IsActive = testUser.IsActive;
                result.Username = testUser.Username;
                result.ProviderName = testUser.ProviderName;
                return result;
            }

            public TestUser ToTestUser()
            {
                return new TestUser
                {
                    SubjectId = SubjectId,
                    Password = Password,
                    IsActive = IsActive,
                    ProviderSubjectId = ProviderSubjectId,
                    Username = Username,
                    ProviderName = ProviderName,
                };
            }

            public string ProviderName { get; set; }

            public string Username { get; set; }

            public bool IsActive { get; set; }

            public string Password { get; set; }

            public string ProviderSubjectId { get; set; }

            public string SubjectId { get; set; }
        }

        public SimpleDbUserStore(IPasswordFunctions passwordFunctions, SimpleDbBasedStore<TestUserDto> simpleDbStore, SimpleDbBasedStore<UserInformation> simpleDbBasedStoreUserInfo)
        {
            _simpleDbStore = simpleDbStore;
            _simpleDbBasedStoreUserInfo = simpleDbBasedStoreUserInfo;
            _passwordFunctions = passwordFunctions;
        }

        public TestUser ValidateCredentials(string username, string password)
        {
            var user = FindByUsername(username);

            if (user?.Password == null)
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
            if (!Guid.TryParse(subjectId, out _))
            {
                return null;
            }

            return _simpleDbStore.FindByItemName(subjectId)?.ToTestUser();
        }
        
        public TestUser FindByUsername(string username)
        {
            username = CanonicalizeUsername(username);
            return _simpleDbStore.SelectItemsBySimpleFilter(x => x.Username, username)
                .FirstOrDefault()
                ?.ToTestUser();
        }

        public Task Store(TestUser user)
        {
            var existing = FindByUsername(user.Username);
            if (existing != null && existing.SubjectId != user.SubjectId)
            {
                throw new ArgumentException("Duplicate users by username");
            }

            string password;
            if (user.Password == null)
            {
                password = null;
            }
            else
            {
                password = _passwordFunctions.IsHashedWithThisFunction(user.Password)
                    ? user.Password
                    : _passwordFunctions.CreateHash(user.Password);
            }

            var dto = TestUserDto.From(user, password);
            dto.Username = CanonicalizeUsername(dto.Username);

            return _simpleDbStore.Store(dto);
        }

        public Task StoreUserInformation(UserInformation userInfo)
        {
            return _simpleDbBasedStoreUserInfo.Store(userInfo);
        }

        public Task<UserInformation> GetUserInformation(string subjectId)
        {
            return Task.FromResult(_simpleDbBasedStoreUserInfo.FindByItemName(subjectId));
        }

        public async Task DeleteBySubjectId(string subjectId)
        {
            await _simpleDbStore.DeleteByItemName(subjectId);
            await _simpleDbBasedStoreUserInfo.DeleteByItemName(subjectId);
        }

        private static string CanonicalizeUsername(string username)
        {
            return username.ToLower(CultureInfo.InvariantCulture);
        }
    }
}
