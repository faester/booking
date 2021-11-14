using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Xunit;
using FluentAssertions;
using IdentityServer.Quickstart;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Test;
using IdentityServerHost.Quickstart.UI;
using Moq;

namespace IdentityServer.unittests.QuickStart
{
    public class SimpleDbUserStoreTests
    {
        public class SimpleDbUserStoreTestsSetup
        {
            private static Lazy<IAmazonSimpleDB> _instance;
            public static Lazy<IAmazonSimpleDB> SimpleDbClient => _instance ??= new Lazy<IAmazonSimpleDB>(Initialize);
            public const string DomainForTest = "integrationtests";

            private static IAmazonSimpleDB Initialize()
            {
                var simpleDbClient = new AmazonSimpleDBClient(SecretsRetriever.GetCredentials(), RegionEndpoint.EUWest1);
                var result = simpleDbClient.ListDomainsAsync();
                result.Wait(TimeSpan.FromSeconds(30));
                if (result.Result.DomainNames.Contains(DomainForTest))
                {
                    simpleDbClient.DeleteDomainAsync(new DeleteDomainRequest(DomainForTest)).Wait();
                }

                simpleDbClient.CreateDomainAsync(new CreateDomainRequest(DomainForTest)).Wait();
                return simpleDbClient;
            }
        }

        private Mock<IPasswordFunctions> _pwdFunctions = new Mock<IPasswordFunctions>();

        private readonly SimpleDbUserStore _subject;

        public SimpleDbUserStoreTests()
        {
            _subject = new SimpleDbUserStore(SimpleDbUserStoreTestsSetup.SimpleDbClient.Value, _pwdFunctions.Object, SimpleDbUserStoreTestsSetup.DomainForTest);
        }

        [Fact]
        public async Task Store_WhenPasswordIsNull()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username);
            actual.SubjectId.Should().Be(original.SubjectId);
        }

        [Fact]
        public async Task Store_WhenDuplicateUsername_ThenException()
        {
            var a = new TestUser
            {
                Username = nameof(Store_WhenDuplicateUsername_ThenException),
                SubjectId = Guid.NewGuid().ToString()
            };
            var b = new TestUser
            {
                Username = nameof(Store_WhenDuplicateUsername_ThenException),
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(a);

            await _subject.Invoking(subj => subj.Store(b)).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task Store_WhenPasswordIsNull_ThenPasswordIsNull()
        {
            var original = new TestUser
            {
                Username = nameof(Store_WhenPasswordIsNull_ThenPasswordIsNull),
                SubjectId = Guid.NewGuid().ToString(),
                Password = null,
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username.ToLower());
            actual.SubjectId.Should().Be(original.SubjectId);
            actual.Password.Should().BeNull();
        }

        [Fact]
        public async Task Store_WhenPasswordIsNull_ThenPasswordIsStoredAsNull()
        {
            var original = new TestUser
            {
                Username = nameof(Store_WhenPasswordIsNull_ThenPasswordIsStoredAsNull),
                SubjectId = Guid.NewGuid().ToString(),
                Password = null,
            };
            _pwdFunctions.Setup(x => x.CreateHash(null)).Returns(null as string);
            _pwdFunctions.Setup(x => x.IsHashedWithThisFunction(It.IsAny<string>())).Returns(true);
            _pwdFunctions.Setup(x => x.CompareHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            await _subject.Store(original);

            var verified = _subject.ValidateCredentials(original.Username, null);
            verified.Should().BeNull();
            _pwdFunctions.Verify(x => x.CreateHash(null), Times.Never);
            _pwdFunctions.Verify(x => x.IsHashedWithThisFunction(It.IsAny<string>()), Times.Never());
            _pwdFunctions.Verify(x => x.CompareHashedPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task Store_WhenPasswordIsNotNull_ThenCorrectPasswordIsVerifiable()
        {
            var original = new TestUser
            {
                Username = nameof(Store_WhenPasswordIsNotNull_ThenCorrectPasswordIsVerifiable),
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            _pwdFunctions.Setup(x => x.CreateHash(It.IsAny<string>())).Returns("hashFromDummy");
            _pwdFunctions.Setup(x => x.IsHashedWithThisFunction(It.IsAny<string>())).Returns(true);
            _pwdFunctions.Setup(x => x.CompareHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            await _subject.Store(original);

            var actual = _subject
                .ValidateCredentials(nameof(Store_WhenPasswordIsNotNull_ThenCorrectPasswordIsVerifiable), original.Password);
            actual.Should().NotBeNull();
            actual.Username.Should().Be(original.Username.ToLower());
            actual.SubjectId.Should().Be(original.SubjectId.ToLower());
            actual.Password.Should().Be("hashFromDummy");
        }

        [Fact]
        public async Task Store_WhenPasswordIsNotNull_ThenIncorrectPasswordIsVerifiable()
        {
            var original = new TestUser
            {
                Username = nameof(Store_WhenPasswordIsNotNull_ThenIncorrectPasswordIsVerifiable),
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            _pwdFunctions.Setup(x => x.IsHashedWithThisFunction(It.IsAny<string>())).Returns(true);
            _pwdFunctions.Setup(x => x.CompareHashedPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            await _subject.Store(original);

            _subject
                .ValidateCredentials(nameof(Store_WhenPasswordIsNotNull_ThenCorrectPasswordIsVerifiable), original.Password)
                .Should()
                .BeNull(because: "password is wrong");
        }

        [Fact]
        public async Task FindByUsername()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)" + nameof(FindByUsername),
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username.ToLower());
        }

        [Fact]
        public async Task FindByUsername_ThenValuesArePreserved()
        {
            var original = new TestUser
            {
                Username = "faester",
                SubjectId = Guid.NewGuid().ToString(),
                Claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Actor, "actor", ClaimValueTypes.String), new Claim(ClaimTypes.Country, "DK", ClaimValueTypes.String),
                },
                Password = "3jFOJZEZ+jK28BN9",
                IsActive = true
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username);
            actual.Claims.Should().AllBeEquivalentTo(original.Claims);
            actual.IsActive.Should().Be(original.IsActive);
            actual.Password.Should().NotBe(original.Password);
            actual.ProviderName.Should().Be(original.ProviderName);
            actual.ProviderSubjectId.Should().Be(original.ProviderSubjectId);
            actual.SubjectId.Should().Be(original.SubjectId);
        }

        [Fact]
        public async Task FindByUsername_ThenIsNotCaseSensitive()
        {
            var original = new TestUser
            {
                Username = "faester2",
                SubjectId = Guid.NewGuid().ToString(),
                Claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Actor, "actor", ClaimValueTypes.String), new Claim(ClaimTypes.Country, "DK", ClaimValueTypes.String),
                },
                Password = "3jFOJZEZ+jK28BN9",
                IsActive = true
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username.ToUpper());

            actual.Username.Should().Be(original.Username);
            actual.Claims.Should().AllBeEquivalentTo(original.Claims);
            actual.IsActive.Should().Be(original.IsActive);
            actual.Password.Should().NotBe(original.Password);
            actual.ProviderName.Should().Be(original.ProviderName);
            actual.ProviderSubjectId.Should().Be(original.ProviderSubjectId);
            actual.SubjectId.Should().Be(original.SubjectId);
        }

        [Fact]
        public async Task Store_ThenCanBeFoundByUsername()
        {
            var original = new TestUser
            {
                Username = nameof(Store_ThenCanBeFoundByUsername),
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username.ToLower(CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task Store_WhenNoSubjectId()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = null
            };

            await _subject.Invoking(s => s.Store(original)).Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task DeleteBySubjectId_ThenDataIsGone()
        {
            var original = new TestUser
            {
                Username = nameof(DeleteBySubjectId_ThenDataIsGone),
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(original);
            var userInfo = new UserInformation();
            userInfo.Address = "Strunseegade 53, 3";
            userInfo.Email = "oofo@gmail.com";
            userInfo.Phone = "+4529641657";
            await _subject.StoreUserInformation(original.SubjectId, userInfo);

            await _subject.DeleteBySubjectId(original.SubjectId);

            (await _subject.GetUserInformation(original.SubjectId)).Should().BeNull("GetByUserInformation");
            _subject.FindBySubjectId(original.SubjectId).Should().BeNull("FindBySubjectId should not fetch user");
            _subject.FindByUsername(original.Username).Should().BeNull("FindByUsername");
        }

        [Fact]
        public async Task StoreUserInformation()
        {
            string subjectId = Guid.NewGuid().ToString();
            UserInformation userInfo = new UserInformation
            {
                Address = "My test address", Email = "faester@gmail.com", Phone = "+4576767676"
            };

            await _subject.StoreUserInformation(subjectId , userInfo);

            var retrieved = await _subject.GetUserInformation(subjectId);
            retrieved.Phone.Should().Be(userInfo.Phone);
            retrieved.Address.Should().Be(userInfo.Address);
            retrieved.Email.Should().Be(userInfo.Email);
        }
    }
}
