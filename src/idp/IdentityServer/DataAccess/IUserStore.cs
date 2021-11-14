using System.Threading.Tasks;
using IdentityServer.Quickstart.Account;
using IdentityServer4.Test;

namespace IdentityServer.DataAccess
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
        Task Store(TestUser user);

        Task StoreUserInformation(UserInformation signupViewModel);
        Task<UserInformation> GetUserInformation(string subjectId);
    }
}
