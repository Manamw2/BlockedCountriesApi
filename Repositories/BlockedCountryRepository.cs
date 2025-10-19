using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Repositories
{
    public class BlockedCountryRepository : IBlockedCountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();
        private readonly ConcurrentDictionary<string, TemporalBlock> _temporalBlocks = new();

        public Task<bool> AddBlockedCountryAsync(BlockedCountry country)
        {
            return Task.FromResult(_blockedCountries.TryAdd(country.CountryCode.ToUpper(), country));
        }

        public Task<bool> RemoveBlockedCountryAsync(string countryCode)
        {
            return Task.FromResult(_blockedCountries.TryRemove(countryCode.ToUpper(), out _));
        }

        public Task<List<BlockedCountry>> GetAllBlockedCountriesAsync(int page, int pageSize, string searchTerm)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CountryCode.Contains(searchTerm.ToUpper()) ||
                    c.CountryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }

            var result = query
                .OrderBy(c => c.CountryCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(result);
        }

        public Task<bool> IsCountryBlockedAsync(string countryCode)
        {
            return Task.FromResult(
                _blockedCountries.ContainsKey(countryCode.ToUpper()) ||
                IsTemporallyBlockedAsync(countryCode).Result
            );
        }

        public Task<bool> AddTemporalBlockAsync(TemporalBlock block)
        {
            return Task.FromResult(_temporalBlocks.TryAdd(block.CountryCode.ToUpper(), block));
        }

        public Task<List<TemporalBlock>> GetExpiredTemporalBlocksAsync()
        {
            var expired = _temporalBlocks.Values
                .Where(b => b.ExpiresAt <= DateTime.UtcNow)
                .ToList();
            return Task.FromResult(expired);
        }

        public Task RemoveTemporalBlockAsync(string countryCode)
        {
            _temporalBlocks.TryRemove(countryCode.ToUpper(), out _);
            return Task.CompletedTask;
        }

        public Task<bool> IsTemporallyBlockedAsync(string countryCode)
        {
            if (_temporalBlocks.TryGetValue(countryCode.ToUpper(), out var block))
            {
                return Task.FromResult(block.ExpiresAt > DateTime.UtcNow);
            }
            return Task.FromResult(false);
        }

        public Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode)
        {
            if (_blockedCountries.TryGetValue(countryCode.ToUpper(), out var block))
            {
                return Task.FromResult<BlockedCountry?>(block);
            }
            return Task.FromResult<BlockedCountry?>(null);
        }

        public Task<int> GetTotalCountAsync(string searchTerm = null)
        {
            var query = _blockedCountries.Values.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CountryCode.Contains(searchTerm.ToUpper()) ||
                    c.CountryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            return Task.FromResult(query.Count());
        }
    }
}
