using BlockedCountriesApi.Models.External;

namespace BlockedCountriesApi.Services.Interfaces
{
    public interface IGeolocationService
    {
        Task<IPGeolocationResponse> GetCountryByIpAsync(string ipAddress);
    }
}
