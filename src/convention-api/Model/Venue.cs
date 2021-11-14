using System;
using System.Runtime.Serialization;
using convention_api.Clients.BreweryClient;

namespace convention_api.Model
{
    [DataContract]
    public class Venue
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "capacity")]
        public int Capacity { get; set; }
        [DataMember(Name = "city")]
        public string City { get; set; }
        [DataMember(Name = "state")]
        public string State { get; set; }
        [DataMember(Name = "street_address")]
        public string StreetAddress { get; set; }
        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        internal static Venue From(Brewery brewery)
        {
            return new Venue
            {
                Capacity = InterpolateCapacity(brewery.BreweryType),
                City = brewery.City,
                Phone = brewery.Phone,
                Name = brewery.Name,
                StreetAddress = brewery.Street,
                State = brewery.State,
            };
        }

        private static int InterpolateCapacity(BreweryType breweryBreweryType)
        {
            switch (breweryBreweryType)
            {
                case BreweryType.None:
                case BreweryType.Micro:
                    return 30;
                case BreweryType.Nano:
                    return 10;
                case BreweryType.Regional:
                    return 100;
                case BreweryType.Brewpub:
                    return 50;
                case BreweryType.Large:
                    return 500;
                case BreweryType.Planning:
                    return 0;
                case BreweryType.Bar:
                    return 20;
                case BreweryType.Contract:
                    return 0;
                case BreweryType.Proprietor:
                    return 0;
                case BreweryType.Closed:
                    return 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(breweryBreweryType), breweryBreweryType, null);
            }
        }
    }
}
