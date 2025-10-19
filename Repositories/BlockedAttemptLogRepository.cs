using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Repositories
{
    public class BlockedAttemptLogRepository : IBlockedAttemptLogRepository
    {
        private readonly ConcurrentBag<BlockedAttemptLog> _logs = new();

        public Task AddLogAsync(BlockedAttemptLog log)
        {
            log.Id = Guid.NewGuid();
            log.Timestamp = DateTime.UtcNow;
            _logs.Add(log);
            return Task.CompletedTask;
        }

        public Task<List<BlockedAttemptLog>> GetLogsAsync(int page, int pageSize)
        {
            var result = _logs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<int> GetTotalCountAsync()
        {
            return Task.FromResult(_logs.Count);
        }
    }
}
