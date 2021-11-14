using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace convention_api.Clients.BreweryClient
{
    [DataContract]
    public class Brewery
    {
        [DataMember(Name="id")]
        public string Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "brewery_type")]
        public BreweryType BreweryType { get; set; }
        [DataMember(Name = "street")]
        public string Street { get; set; }
        [DataMember(Name = "address_2")]
        public object Address2 { get; set; }
        [DataMember(Name = "address_3")]
        public object Address3 { get; set; }
        [DataMember(Name = "city")]
        public string City { get; set; }
        [DataMember(Name = "state")]
        public string State { get; set; }
        [DataMember(Name = "county_province")]
        public object CountyProvince { get; set; }
        [DataMember(Name = "postal_code")]
        public string PostalCode { get; set; }
        [DataMember(Name = "country")]
        public string Country { get; set; }
        [DataMember(Name = "longitude")]
        public string Longitude { get; set; }
        [DataMember(Name = "latitude")]
        public string Latitude { get; set; }
        [DataMember(Name = "phone")]
        public string Phone { get; set; }
        [DataMember(Name = "website_url")]
        public string WebsiteUrl { get; set; }
        [DataMember(Name = "updated_at")]
        public DateTime UpdatedAt { get; set; }
        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }
    }
}