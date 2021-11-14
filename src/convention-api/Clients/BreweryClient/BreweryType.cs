using System.Runtime.Serialization;

namespace convention_api.Clients.BreweryClient
{
    public enum BreweryType
    {
        None,
        [EnumMember(Value = "micro")]
        Micro,
        [EnumMember(Value = "nano")]
        Nano,
        [EnumMember(Value = "regional")]
        Regional,
        [EnumMember(Value = "brewpub")]
        Brewpub,
        [EnumMember(Value = "large")]
        Large,
        [EnumMember(Value = "planning")]
        Planning,
        [EnumMember(Value = "bar")]
        Bar,
        [EnumMember(Value = "contract")]
        Contract,
        [EnumMember(Value = "proprietor")]
        Proprietor,
        [EnumMember(Value = "closed")]
        Closed,
    }
}