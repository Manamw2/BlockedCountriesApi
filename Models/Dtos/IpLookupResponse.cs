namespace BlockedCountriesApi.Models.Dtos
{
    public class IpLookupResponse
    {
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string Isp { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string CountryFlag { get; set; }
    }
}
