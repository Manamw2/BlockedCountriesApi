using System.ComponentModel.DataAnnotations;

namespace BlockedCountriesApi.Models.Dtos
{
    public class BlockCountryRequest
    {
        [Required]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country code must be 2 uppercase letters")]
        public string CountryCode { get; set; }
    }
}
