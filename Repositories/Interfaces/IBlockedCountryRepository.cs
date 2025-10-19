using BlockedCountriesApi.Models.Entities;

namespace BlockedCountriesApi.Repositories.Interfaces
{
    public interface IBlockedCountryRepository
    {
        Task<bool> AddBlockedCountryAsync(BlockedCountry country);
        Task<bool> RemoveBlockedCountryAsync(string countryCode);
        Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode);
        Task<List<BlockedCountry>> GetAllBlockedCountriesAsync(int page, int pageSize, string? searchTerm = null);
        Task<int> GetTotalCountAsync(string? searchTerm = null);
        Task<bool> IsCountryBlockedAsync(string countryCode);
        Task<List<BlockedCountry>> GetExpiredTemporalBlocksAsync();
    }
}
