using System.Globalization;

namespace BlockedCountriesApi.Helpers
{
    public static class CountryHelper
    {
        /// <summary>
        /// Gets the country name from a country code
        /// </summary>
        /// <param name="countryCode">The 2-letter country code</param>
        /// <returns>The country name, or the country code itself if not found</returns>
        public static string GetCountryName(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode))
            {
                return string.Empty;
            }

            var upperCode = countryCode.ToUpper();
            try
            {
                    var region = new RegionInfo(countryCode);
                    return region.EnglishName;
                }
            catch
            {
                    return countryCode; // Fallback if invalid
            }
        }
    }
}

