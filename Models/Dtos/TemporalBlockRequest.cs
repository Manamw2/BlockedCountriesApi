using System.ComponentModel.DataAnnotations;

namespace BlockedCountriesApi.Models.Dtos
{
    public class TemporalBlockRequest
    {
        [Required]
        [RegularExpression(@"^[A-Z]{2}$")]
        public string CountryCode { get; set; }

        [Range(1, 1440, ErrorMessage = "Duration must be between 1 and 1440 minutes")]
        public int DurationMinutes { get; set; }
    }
}
