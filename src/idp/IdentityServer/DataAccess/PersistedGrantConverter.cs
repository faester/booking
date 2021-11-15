using System;
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
}