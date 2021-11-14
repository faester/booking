using System;
using System.Linq.Expressions;
using ConventionApiLibrary.DataAccess;
using IdentityServer4.Models;

namespace IdentityServer.DataAccess
{
    public class PersistedGrantConverter : BaseConverter<PersistedGrant>
    {
        public PersistedGrantConverter()
        : base(x => new PersistedGrant{Key = x}, pg => pg.Key)
        {
            AddFieldMapping("SubjectId",
                (g, v) => g.SubjectId = v,
                g => g.SubjectId,
                g => g.SubjectId
            );
            AddFieldMapping("ClientId",
                (g, v) => g.ClientId = v,
                g => g.ClientId,
                g => g.ClientId
            );
            AddFieldMapping("SessionId",
                (g, v) => g.SessionId = v,
                g => g.SessionId,
                g => g.SessionId
            );
            AddFieldMapping("ConsumedTime",
                (g, v) => g.ConsumedTime = DateTime.TryParse(v, out var tmp) ? tmp : (DateTime?)null,
                g => g.ConsumedTime?.ToString("O"),
                g => g.ConsumedTime
            );
            AddFieldMapping("CreationTime",
                (g, v) => g.CreationTime = DateTime.Parse(v),
                g => g.CreationTime.ToString("O"),
                g => g.CreationTime
            );
            AddFieldMapping("Data",
                (g, v) => g.Data = v,
                g => g.Data,
                g => g.Data
            );
            AddFieldMapping("Description",
                (g, v) => g.Description = v,
                g => g.Description,
                g => g.Description
            );
            AddFieldMapping("Expiration",
                (g, v) => g.Expiration = DateTime.TryParse(v, out var tmp) ? tmp : (DateTime?)null,
                g => g.Expiration?.ToString("O"),
                g => g.Expiration
            );
            AddFieldMapping("Type",
                (g, v) => g.Type = v,
                g => g.Type,
                g => g.Type
            );
        }
    }

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