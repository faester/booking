using System;
using System.Linq.Expressions;
using ConventionApiLibrary.DataAccess;
using IdentityServer.Quickstart.Account;

namespace IdentityServer.DataAccess
{
    public class UserInfoConverter : BaseConverter<UserInformation>
    {
        public UserInfoConverter() 
            : base(x => new UserInformation{SubjectId = x}, ui => ui.SubjectId)
        {
            AddFieldMapping("address", 
                (x, v) => x.Address = v, 
                x => x.Address,
                x => x.Address);
            AddFieldMapping("phone", 
                (x, v) => x.Phone = v,
                x => x.Phone,
                x => x.Phone);
            AddFieldMapping("email",
                (x, v) => x.Email = v,
                x => x.Email,
                x => x.Email);
            AddFieldMapping("is_administrator",
                (x, v) => x.IsAdministrator = v == "true",
                x => x.IsAdministrator ? "true" : "false",
                x => x.IsAdministrator);
            AddFieldMapping("is_speaker",
                (x, v) => x.IsSpeaker = v == "true",
                x => x.IsSpeaker ? "true" : "false",
                x => x.IsSpeaker);
            AddFieldMapping("is_validated_user",
                (x, v) => x.IsValidatedUser = v == "true",
                x => x.IsValidatedUser ? "true" : "false",
                x => x.IsValidatedUser);
        }
    }
}