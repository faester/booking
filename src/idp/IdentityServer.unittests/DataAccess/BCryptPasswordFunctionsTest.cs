using FluentAssertions;
using IdentityServer.DataAccess;
using Xunit;

namespace IdentityServer.unittests.DataAccess
{
    public class BCryptPasswordFunctionsTest
    {
        private readonly BCryptPasswordFunctions _subject = new BCryptPasswordFunctions();

        [Fact]
        public void CreateHash_ThenNotInput()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";

            var hash = _subject.CreateHash(pwd);

            hash.Should().NotBe(pwd);
        }

        [Fact]
        public void CompareHashedPassword_WhenIdentical()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";
            var hash = _subject.CreateHash(pwd);

            _subject.CompareHashedPassword(pwd, hash)
                .Should().BeTrue();
        }

        [Fact]
        public void CompareHashedPassword_WhenDifferent()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";
            var hash = _subject.CreateHash(pwd);

            _subject.CompareHashedPassword(pwd + "changed", hash)
                .Should().BeFalse();
        }

        [Fact]
        public void CompareHashedPassword_WhenNullCleartext()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";
            var hash = _subject.CreateHash(pwd);

            _subject.CompareHashedPassword(null, hash)
                .Should().BeFalse();
        }

        [Fact]
        public void CompareHashedPassword_WhenNullPasswordHash()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";

            _subject.CompareHashedPassword(pwd + "changed", null)
                .Should().BeFalse();
        }

        [Fact]
        public void IsHashedWithThisFunction_WhenRandomString()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";

            _subject.IsHashedWithThisFunction(pwd)
                .Should().BeFalse();
        }

        [Fact]
        public void IsHashedWithThisFunction_WhenActualHash()
        {
            var pwd = "90asdaljd*?!\"¤\"! M WÆELQEQ";
            var hash = _subject.CreateHash(pwd);
            _subject.IsHashedWithThisFunction(hash)
                .Should().BeTrue();
        }
    }
}