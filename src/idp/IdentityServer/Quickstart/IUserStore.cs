using System.Threading.Tasks;
using IdentityServer4.Test;

namespace IdentityServer.Quickstart
{
    public interface IUserStore
    {
        /// <summary>
        /// Returns a user with matching username and password if such
        /// a user exists. Otherwise null is returned. 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        TestUser ValidateCredentials(string username, string password);
        TestUser FindBySubjectId(string subjectId);
        TestUser FindByUsername(string username);
        TestUser FindByExternalProvider(string provider, string userId);
        Task Store(TestUser user);
    }
}
