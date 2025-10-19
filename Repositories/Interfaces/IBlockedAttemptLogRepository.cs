using BlockedCountriesApi.Models.Entities;

namespace BlockedCountriesApi.Repositories.Interfaces
{
    public interface IBlockedAttemptLogRepository
    {
        Task AddLogAsync(BlockedAttemptLog log);
        Task<List<BlockedAttemptLog>> GetLogsAsync(int page, int pageSize);
        Task<int> GetTotalCountAsync();
    }
}
