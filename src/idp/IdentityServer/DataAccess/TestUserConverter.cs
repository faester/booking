using System;
using System.Linq.Expressions;
using ConventionApiLibrary.DataAccess;

namespace IdentityServer.DataAccess
{
    public class TestUserConverter : BaseConverter<SimpleDbUserStore.TestUserDto>, ISimpleDbConverter<SimpleDbUserStore.TestUserDto>
    {
        private const string USERNAME_FIELD = "username";
        private const string IS_ACTIVE = "isActive";
        private const string PROVIDERNAME_FIELD = "provider";
        private const string PROVIDER_SUBJECTID_FIELD = "provider_subject";
        private const string PASSWORD_FIELD = "pwd";

        public TestUserConverter() 
            : base(itemName => new SimpleDbUserStore.TestUserDto(){SubjectId = itemName}, tu => tu.SubjectId)
        {
            AddFieldMapping(USERNAME_FIELD, 
                (tu, value) => tu.Username = value, 
                tu => tu.Username,
                        x => x.Username);
            AddFieldMapping(IS_ACTIVE, 
                (tu, value) => tu.IsActive = value == "true",
                tu => tu.IsActive ? "true" : "false",
                        tu => tu.IsActive);
            AddFieldMapping(PROVIDERNAME_FIELD, 
                (tu, value) => tu.ProviderName = value,
                tu => tu.ProviderName,
                        tu => tu.ProviderName);
            AddFieldMapping(PROVIDER_SUBJECTID_FIELD, 
                (tu, value) => tu.ProviderSubjectId = value, 
                tu => tu.ProviderSubjectId,
                        tu => tu.ProviderSubjectId);
            AddFieldMapping(PASSWORD_FIELD, 
                (tu, value) => tu.Password = value,
                tu => tu.Password,
                        tu => tu.Password);
        }

        public string GetSimpleDbFieldNameFor(Expression<Func<SimpleDbUserStore.TestUserDto, object>> dtoField)
        {
            return base.GetSimpleDbName(dtoField);
        }
    }
}