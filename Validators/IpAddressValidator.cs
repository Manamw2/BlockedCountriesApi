using System.Net;

namespace BlockedCountriesApi.Validators
{
    public static class IpAddressValidator
    {
        public static bool IsValid(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            return IPAddress.TryParse(ipAddress, out _);
        }
    }
}
