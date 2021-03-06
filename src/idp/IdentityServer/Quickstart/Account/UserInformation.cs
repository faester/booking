using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IdentityServer.Quickstart.Account
{
    public class UserInformation
    {
        public string SubjectId { get; set; }

        [Required]
        [RegularExpression(pattern: @"[a-z0-9_.-]+@[a-z0-9_.-]+\.[a-z]{2,}", ErrorMessage = "Email does not look like an e-mail")]
        public string Email { get; set; }

        [Required]
        [RegularExpression(pattern: @"\+[0-9 ]+", ErrorMessage = "Phone is not valid. Numbers and spaces only. ")]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        public bool IsAdministrator { get; set; }
        public bool IsSpeaker { get; set; }
        public bool IsValidatedUser { get; set; }

        public IEnumerable<Claim> GetClaims()
        {
            if (Email != null)
            {
                yield return new Claim("email", Email);
            }

            if (Address != null)
            {
                yield return new Claim( "address", Address);
            }

            if (Phone != null)
            {
                yield return new Claim("phone", Phone);
            }

            if (IsAdministrator)
            {
                yield return new Claim("role", "administrator");
            }
            if (IsSpeaker)
            {
                yield return new Claim("role", "speaker");
            }
            if (IsValidatedUser)
            {
                yield return new Claim("role", "validated_user");
            }
        }
    }
}