using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Xunit;
using FluentAssertions;
using IdentityServer.Quickstart;
using IdentityServer4.Test;

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


        private readonly SimpleDbUserStore _subject;

        public SimpleDbUserStoreTests()
        {
            _subject = new SimpleDbUserStore(SimpleDbUserStoreTestsSetup.SimpleDbClient.Value, SimpleDbUserStoreTestsSetup.DomainForTest);
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
        public async Task Store_WhenPasswordIsNull_ThenPasswordIsNotIdentical()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username);
            actual.SubjectId.Should().Be(original.SubjectId);
            actual.Password.Should().NotBe(original.Password);
        }

        [Fact]
        public async Task Store_WhenPasswordIsNull_ThenPasswordIsVerifiable()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            await _subject.Store(original);

            throw new NotImplementedException();
        }

        [Fact]
        public async Task Store_WhenPasswordIsNotNull_ThenCorrectPasswordIsVerifiable()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            await _subject.Store(original);

            throw new NotImplementedException();
        }


        [Fact]
        public async Task Store_WhenPasswordIsNotNull_ThenIncorrectPasswordIsVerifiable()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString(),
                Password = "HelloWorld021938",
            };
            await _subject.Store(original);

            throw new NotImplementedException();
        }

        [Fact]
        public async Task FindByUsername()
        {
            var original = new TestUser();
            original.Username = "fancy fancy - tak :)";
            original.SubjectId = Guid.NewGuid().ToString();
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username);
        }

        [Fact]
        public async Task FindByUsername_ThenValuesArePreserved()
        {
            var original = new TestUser();
            original.Username = "faester";
            original.SubjectId = Guid.NewGuid().ToString();
            original.Claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Actor, "actor", ClaimValueTypes.String), new Claim(ClaimTypes.Country, "DK", ClaimValueTypes.String),
            };
            original.Password = "3jFOJZEZ+jK28BN9";
            original.IsActive = true;
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
        public async Task Store_ThenCanBeFoundByUsername()
        {
            var original = new TestUser
            {
                Username = "fancy fancy - tak :)",
                SubjectId = Guid.NewGuid().ToString()
            };
            await _subject.Store(original);

            var actual = _subject.FindByUsername(original.Username);

            actual.Username.Should().Be(original.Username);
        }

        [Fact]
        public async Task Store_WhenNoSubjectId()
        {
            var original = new TestUser();
            original.Username = "fancy fancy - tak :)";
            original.SubjectId = null;

            await _subject.Invoking(s => s.Store(original)).Should().ThrowAsync<ArgumentException>();
        }
    }
}
