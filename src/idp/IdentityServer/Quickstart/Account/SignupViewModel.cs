using System.ComponentModel.DataAnnotations;
using IdentityServer.Quickstart.Account;

namespace IdentityServerHost.Quickstart.UI
{
    public class SignupViewModel : UserInformation
    {
        [Required]
        [RegularExpression(pattern : "[a-z0-9_-]{3,}", ErrorMessage = "Username must be at least 3 characters and contain alphanumerics, dashes and underscores")]
        public string Username { get; set; }
        [Required]
        [RegularExpression(pattern : "(.){6,}", ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
        public bool TermsAccepted { get; set; }
        public string ReturnUrl { get; set; }
    }
}