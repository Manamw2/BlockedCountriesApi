namespace BlockedCountriesApi.Models.Entities
{
    public class BlockedCountry
    {
        public required string CountryCode { get; set; }
        public required string CountryName { get; set; }
        public DateTime BlockedAt { get; set; }
        public bool IsTemporary { get; set; }
        public int? DurationMinutes { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
