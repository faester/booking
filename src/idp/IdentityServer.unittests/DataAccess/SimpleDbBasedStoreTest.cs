using System;
using System.Linq;
using System.Threading.Tasks;
using ConventionApiLibrary.DataAccess;
using FluentAssertions;
using Xunit;

namespace IdentityServer.unittests.DataAccess
{
    public class SimpleDbBasedStoreTest
    {
        SimpleDbBasedStore<TestDataClass> _subject = new SimpleDbBasedStore<TestDataClass>(
            SimpleDbUserStoreTests.SimpleDbUserStoreTestsSetup.SimpleDbClient.Value,
            SimpleDbUserStoreTests.SimpleDbUserStoreTestsSetup.DomainForTest,
            new TestDataClassConverter());

        public class TestDataClass
        {
            public Guid Key { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }


        internal class TestDataClassConverter : BaseConverter<SimpleDbBasedStoreTest.TestDataClass>
        {
            public TestDataClassConverter() : base(
                x => new SimpleDbBasedStoreTest.TestDataClass
                {
                    Key = Guid.Parse(x)
                },
                t => t.Key.ToString())
            {
                AddFieldMapping("FirstName", (x, v) => x.FirstName = v, x => x.FirstName, x => x.FirstName);
                AddFieldMapping("LastName", (x, v) => x.LastName = v, x => x.LastName, x => x.LastName);
            }
        }

        [Fact]
        public async Task Where_WhenSingleCriteria()
        {
            TestDataClass[] items = {
                new TestDataClass
                {
                    FirstName = "Anders", LastName = "Jensen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Anders", LastName = "Larsen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Poul", LastName = "Jensen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Poul", LastName = "Larsen", Key = Guid.NewGuid()
                },
            };
            var storeTasks = items.Select(x => _subject.Store(x)).ToArray();
            await Task.WhenAll(storeTasks);

            var actual = _subject
                .Where(x => x.FirstName, "Anders")
                .Select().Select(x => x.Key);
            actual.Should().BeEquivalentTo(new[]
            {
                items[0].Key, items[1].Key
            });
        }

        [Fact]
        public async Task Where_WhenMultipleCriteria()
        {
            TestDataClass[] items = {
                new TestDataClass
                {
                    FirstName = "Anders", LastName = "Jensen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Anders", LastName = "Larsen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Poul", LastName = "Jensen", Key = Guid.NewGuid()
                },
                new TestDataClass
                {
                    FirstName = "Poul", LastName = "Larsen", Key = Guid.NewGuid()
                },
            };
            var storeTasks = items.Select(x => _subject.Store(x)).ToArray();
            await Task.WhenAll(storeTasks);

            var actual = _subject
                .Where(x => x.FirstName, "Anders")
                .AndAlso(x => x.LastName, "Larsen")
                .Select().Select(x => x.Key);
            actual.Should().BeEquivalentTo(new[]
            {
                items[1].Key
            });
        }
    }
}