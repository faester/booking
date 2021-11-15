using System;
using System.Runtime.CompilerServices;
using System.Text;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using ConventionApiLibrary.DataAccess;
using IdentityServer4.Models;

namespace IdentityServer.DataAccess
{
    public class PersistedGrantDto
    {
        public PersistedGrant ToPersistedGrant()
        {
            PersistedGrant result = new PersistedGrant();
            result.SubjectId = SubjectId;
            result.ClientId = ClientId;
            result.ConsumedTime = ConsumedTime;
            result.CreationTime = CreationTime;
            result.Data = GetData();
            result.Description = Description;
            result.Expiration = Expiration;
            result.SessionId = SessionId;
            result.Type = Type;
            result.Key = Key;
            return result;
        }

        public string SessionId { get; set; }

        public string Type { get; set; }

        public static PersistedGrantDto FromPersistedGrant(PersistedGrant original)
        {
            var result = new PersistedGrantDto();
            result.SubjectId = original.SubjectId;
            result.ClientId = original.ClientId;
            result.CreationTime = original.CreationTime;
            result.ConsumedTime = original.ConsumedTime;
            result.Description = original.Description;
            result.PopulateData(original.Data);
            result.SessionId = original.SessionId;
            result.Type = original.Type;
            result.Key = original.Key;
            return result;
        }

        private void PopulateData(string originalData)
        {
            Data0 = LargestValidSubstring(originalData, 0, 1000);
            Data1 = LargestValidSubstring(originalData, 1000, 1000);
            Data2 = LargestValidSubstring(originalData, 2000, 1000);
            Data3 = LargestValidSubstring(originalData, 3000, 1000);
            Data4 = LargestValidSubstring(originalData, 4000, 1000);
        }

        private string LargestValidSubstring(string s, int startIndex, int length)
        {
            if (s.Length <= startIndex)
            {
                return null;
            }

            var actualLength = Math.Min(length, s.Length - startIndex);

            return s.Substring(startIndex, actualLength);
        }

        public DateTime? Expiration { get; set; }

        public string Description { get; set; }

        private string GetData()
        {
            var sb = new StringBuilder();
            if (Data0 != null)
            {
                sb.Append(Data0);
            }
            if (Data1 != null)
            {
                sb.Append(Data1);
            }
            if (Data2 != null)
            {
                sb.Append(Data2);
            }
            if (Data3 != null)
            {
                sb.Append(Data3);
            }
            if (Data4 != null)
            {
                sb.Append(Data4);
            }

            return sb.ToString();
        }

        public string Data0 { get; set; }
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Data3 { get; set; }
        public string Data4 { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? ConsumedTime { get; set; }

        public string ClientId { get; set; }

        public string SubjectId { get; set; }
        public string Key { get; set; }
    }

    public class PersistedGrantDtoConverter : BaseConverter<PersistedGrantDto>
    {
        public PersistedGrantDtoConverter()
            : base(x => new PersistedGrantDto{Key = x}, pg => pg.Key)
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
            AddFieldMapping("Data0",
                (g, v) => g.Data0 = v,
                g => g.Data0,
                g => g.Data0
            );
            AddFieldMapping("Data1",
                (g, v) => g.Data1 = v,
                g => g.Data1,
                g => g.Data1
            );
            AddFieldMapping("Data2",
                (g, v) => g.Data2 = v,
                g => g.Data2,
                g => g.Data2
            );
            AddFieldMapping("Data3",
                (g, v) => g.Data3 = v,
                g => g.Data3,
                g => g.Data3
            );
            AddFieldMapping("Data4",
                (g, v) => g.Data4 = v,
                g => g.Data4,
                g => g.Data4
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