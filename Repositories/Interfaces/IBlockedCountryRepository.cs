using BlockedCountriesApi.Models.Entities;

namespace BlockedCountriesApi.Repositories.Interfaces
{
    public interface IBlockedCountryRepository
    {
        Task<bool> AddBlockedCountryAsync(BlockedCountry country);
        Task<bool> RemoveBlockedCountryAsync(string countryCode);
        Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode);
        Task<List<BlockedCountry>> GetAllBlockedCountriesAsync(int page, int pageSize, string searchTerm = null);
        Task<int> GetTotalCountAsync(string searchTerm = null);
        Task<bool> IsCountryBlockedAsync(string countryCode);

        // Temporal blocks
        Task<bool> AddTemporalBlockAsync(TemporalBlock block);
        Task<List<TemporalBlock>> GetExpiredTemporalBlocksAsync();
        Task RemoveTemporalBlockAsync(string countryCode);
        Task<bool> IsTemporallyBlockedAsync(string countryCode);
    }
}
