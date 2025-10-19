using System.Globalization;

namespace BlockedCountriesApi.Validators
{
    public static class CountryCodeValidator
    {

        /// <summary>
        /// Validates if a country code is valid
        /// </summary>
        /// <param name="countryCode">The country code to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValid(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            {
                return false;
            }

            var upperCode = countryCode.ToUpper();

            try
            {
                // Try to create RegionInfo - will throw if invalid
                _ = new RegionInfo(upperCode);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}

