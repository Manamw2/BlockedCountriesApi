namespace BlockedCountriesApi.Models.Entities
{
    public class BlockedAttemptLog
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; }
        public string CountryCode { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsBlocked { get; set; }
        public string UserAgent { get; set; }
    }
}
