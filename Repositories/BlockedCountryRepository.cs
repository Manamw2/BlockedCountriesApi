using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using System.Collections.Concurrent;

namespace BlockedCountriesApi.Repositories
{
    public class BlockedCountryRepository : IBlockedCountryRepository
    {
        private readonly ConcurrentDictionary<string, BlockedCountry> _blockedCountries = new();

        public Task<bool> AddBlockedCountryAsync(BlockedCountry country)
        {
            return Task.FromResult(_blockedCountries.TryAdd(country.CountryCode.ToUpper(), country));
        }

        public Task<bool> RemoveBlockedCountryAsync(string countryCode)
        {
            return Task.FromResult(_blockedCountries.TryRemove(countryCode.ToUpper(), out _));
        }

        public Task<List<BlockedCountry>> GetAllBlockedCountriesAsync(int page, int pageSize, string? searchTerm = null)
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
            if (_blockedCountries.TryGetValue(countryCode.ToUpper(), out var country))
            {
                // If it's temporary, check if it's still valid (not expired)
                if (country.IsTemporary)
                {
                    return Task.FromResult(country.ExpiresAt.HasValue && country.ExpiresAt.Value > DateTime.UtcNow);
                }
                // Permanent block
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task<List<BlockedCountry>> GetExpiredTemporalBlocksAsync()
        {
            var expired = _blockedCountries.Values
                .Where(b => b.IsTemporary && b.ExpiresAt.HasValue && b.ExpiresAt.Value <= DateTime.UtcNow)
                .ToList();
            return Task.FromResult(expired);
        }

        public Task<BlockedCountry?> GetBlockedCountryAsync(string countryCode)
        {
            if (_blockedCountries.TryGetValue(countryCode.ToUpper(), out var block))
            {
                return Task.FromResult<BlockedCountry?>(block);
            }
            return Task.FromResult<BlockedCountry?>(null);
        }

        public Task<int> GetTotalCountAsync(string? searchTerm = null)
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
