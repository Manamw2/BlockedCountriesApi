namespace BlockedCountriesApi.Models.Dtos
{
    public class BlockCheckResponse
    {
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockType { get; set; } // "Permanent" or "Temporal"
        public string Message { get; set; }
    }
}
