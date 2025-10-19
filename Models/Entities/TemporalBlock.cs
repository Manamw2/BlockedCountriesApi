namespace BlockedCountriesApi.Models.Entities
{
    public class TemporalBlock
    {
        public string CountryCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int DurationMinutes { get; set; }
    }
}
