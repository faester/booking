using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using IdentityServer.Quickstart.Account;
using Xunit;

namespace IdentityServer.unittests.QuickStart.Account
{
    public class UserInformationTest
    {
        [Fact]
        public void GetClaims_ThenAddress()
        {
            UserInformation subject = new UserInformation();
            subject.Address = "Helloworld Road 13, 2";

            var actual = subject.GetClaims();

            actual.Should().Contain(x => x.Type == "address" && x.Value == subject.Address);
        }

        [Fact]
        public void GetClaims_ThenEmail()
        {
            UserInformation subject = new UserInformation();
            subject.Email = "john@doe.com";

            var actual = subject.GetClaims();

            actual.Should().Contain(x => x.Type == "email" && x.Value == subject.Email);
        }

        [Fact]
        public void GetClaims_ThenPhone()
        {
            UserInformation subject = new UserInformation();
            subject.Phone = "+4529292929";

            var actual = subject.GetClaims();

            actual.Should().Contain(x => x.Type == "phone" && x.Value == subject.Phone);
        }

        [Fact]
        public void GetClaims_ThenAll()
        {
            UserInformation subject = new UserInformation();
            subject.Address = "Helloworl Road 13, 2";
            subject.Email = "john@doe.com";
            subject.Phone = "+4529292929";

            var actual = subject.GetClaims().ToArray();

            actual.Should().Contain(x => x.Type == "address" && x.Value == subject.Address);
            actual.Should().Contain(x => x.Type == "email" && x.Value == subject.Email);
            actual.Should().Contain(x => x.Type == "phone" && x.Value == subject.Phone);
        }
    }
}
