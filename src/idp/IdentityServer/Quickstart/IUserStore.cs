using System.Threading.Tasks;
using IdentityServer4.Test;

namespace IdentityServer.Quickstart
{
    public interface IUserStore
    {
        bool ValidateCredentials(string username, string password);
        TestUser FindBySubjectId(string subjectId);
        TestUser FindByUsername(string username);
        TestUser FindByExternalProvider(string provider, string userId);
        Task Store(TestUser user);
        Task StorePassword(string subjectId, string password);
    }
}
