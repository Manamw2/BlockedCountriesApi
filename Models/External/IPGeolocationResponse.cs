using Newtonsoft.Json;

namespace BlockedCountriesApi.Models.External
{
    public class IPGeolocationResponse
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("location")]
        public LocationInfo Location { get; set; }

        [JsonProperty("country_metadata")]
        public CountryMetadata CountryMetadata { get; set; }

        [JsonProperty("currency")]
        public CurrencyInfo Currency { get; set; }
    }

    public class LocationInfo
    {
        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }

        [JsonProperty("continent_name")]
        public string ContinentName { get; set; }

        [JsonProperty("country_code2")]
        public string CountryCode2 { get; set; }

        [JsonProperty("country_code3")]
        public string CountryCode3 { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("country_name_official")]
        public string CountryNameOfficial { get; set; }

        [JsonProperty("country_capital")]
        public string CountryCapital { get; set; }

        [JsonProperty("state_prov")]
        public string StateProv { get; set; }

        [JsonProperty("state_code")]
        public string StateCode { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("is_eu")]
        public bool IsEu { get; set; }

        [JsonProperty("country_flag")]
        public string CountryFlag { get; set; }

        [JsonProperty("geoname_id")]
        public string GeonameId { get; set; }

        [JsonProperty("country_emoji")]
        public string CountryEmoji { get; set; }
    }

    public class CountryMetadata
    {
        [JsonProperty("calling_code")]
        public string CallingCode { get; set; }

        [JsonProperty("tld")]
        public string Tld { get; set; }

        [JsonProperty("languages")]
        public List<string> Languages { get; set; }
    }

    public class CurrencyInfo
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }
    }
}
