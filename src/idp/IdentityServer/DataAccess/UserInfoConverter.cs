using System;
using System.Linq.Expressions;
using ConventionApiLibrary.DataAccess;
using IdentityServer.Quickstart.Account;

namespace IdentityServer.DataAccess
{
    public class UserInfoConverter : BaseConverter<UserInformation>, ISimpleDbConverter<UserInformation>
    {
        private const string ADDRESS_FIELD = "address";
        private const string PHONE_FIELD = "phone";
        private const string EMAIL_FIELD = "email";

        public UserInfoConverter() 
            : base(x => new UserInformation{SubjectId = x}, ui => ui.SubjectId)
        {
            AddFieldMapping(ADDRESS_FIELD, 
                (x, v) => x.Address = v, 
                x => x.Address,
                x => x.Address);
            AddFieldMapping(PHONE_FIELD, 
                (x, v) => x.Phone = v,
                x => x.Phone,
                x => x.Phone);
            AddFieldMapping(EMAIL_FIELD, 
                (x, v) => x.Email = v,
                x => x.Email,
                x => x.Email);
        }

        public string GetSimpleDbFieldNameFor(Expression<Func<UserInformation, object>> dtoField)
        {
            return base.GetSimpleDbName(dtoField);
        }
    }
}