using BlockedCountriesApi.Helpers;
using BlockedCountriesApi.Models.Dtos;
using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using BlockedCountriesApi.Validators;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesApi.Controllers
{
    [ApiController]
    [Route("api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly IBlockedCountryRepository _repository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(
            IBlockedCountryRepository repository,
            ILogger<CountriesController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Add a country to the permanent blocked list
        /// </summary>
        /// <param name="request">The country code to block (e.g., "US", "GB", "EG")</param>
        /// <returns>Success message with blocked country details</returns>
        /// <response code="200">Country successfully blocked</response>
        /// <response code="400">Invalid request or country code format</response>
        /// <response code="409">Country is already blocked</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/countries/block
        ///     {
        ///        "countryCode": "US"
        ///     }
        ///
        /// If the country is temporarily blocked, it will be upgraded to a permanent block.
        /// </remarks>
        [HttpPost("block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> BlockCountry([FromBody] BlockCountryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryCode = request.CountryCode.ToUpper();

            // Check if already blocked
            var existing = await _repository.GetBlockedCountryAsync(countryCode);
            if (existing != null)
            {
                // If it's temporary, upgrade to permanent
                if (existing.IsTemporary)
                {
                    existing.IsTemporary = false;
                    existing.DurationMinutes = null;
                    existing.ExpiresAt = null;
                    existing.BlockedAt = DateTime.UtcNow;

                    _logger.LogInformation("Upgrading temporal block to permanent block for {CountryCode}", countryCode);

                    return Ok(new
                    {
                        message = $"Country {countryCode} has been upgraded from temporal to permanent block.",
                        country = existing,
                        upgraded = true
                    });
                }

                return Conflict(new { message = $"Country {countryCode} is already permanently blocked." });
            }

            var blockedCountry = new BlockedCountry
            {
                CountryCode = countryCode,
                CountryName = CountryHelper.GetCountryName(countryCode),
                BlockedAt = DateTime.UtcNow,
                IsTemporary = false,
                DurationMinutes = null,
                ExpiresAt = null
            };

            var added = await _repository.AddBlockedCountryAsync(blockedCountry);

            if (!added)
            {
                return Conflict(new { message = $"Failed to block country {countryCode}." });
            }

            _logger.LogInformation("Country {CountryCode} has been permanently blocked", countryCode);

            return Ok(new
            {
                message = $"Country {countryCode} has been successfully blocked.",
                country = blockedCountry,
                upgraded = false
            });
        }

        /// <summary>
        /// Remove a country from the blocked list
        /// </summary>
        /// <param name="countryCode">The 2-letter country code to unblock (e.g., "US")</param>
        /// <returns>Success message</returns>
        /// <response code="200">Country successfully unblocked</response>
        /// <response code="400">Invalid country code format</response>
        /// <response code="404">Country is not in the blocked list</response>
        [HttpDelete("block/{countryCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UnblockCountry(string countryCode)
        {
            if (string.IsNullOrWhiteSpace(countryCode) || countryCode.Length != 2)
            {
                return BadRequest(new { message = "Invalid country code. Must be 2 letters." });
            }

            countryCode = countryCode.ToUpper();

            var removed = await _repository.RemoveBlockedCountryAsync(countryCode);

            if (!removed)
            {
                return NotFound(new { message = $"Country {countryCode} is not in the blocked list." });
            }

            _logger.LogInformation("Country {CountryCode} has been unblocked", countryCode);

            return Ok(new { message = $"Country {countryCode} has been successfully unblocked." });
        }

        /// <summary>
        /// Get all blocked countries with pagination and search
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <param name="search">Search term to filter by country code or name</param>
        /// <returns>Paginated list of blocked countries</returns>
        /// <response code="200">Returns the paginated list of blocked countries</response>
        /// <response code="400">Invalid pagination parameters</response>
        [HttpGet("blocked")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBlockedCountries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            if (page < 1)
            {
                return BadRequest(new { message = "Page must be greater than 0." });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Page size must be between 1 and 100." });
            }

            var countries = await _repository.GetAllBlockedCountriesAsync(page, pageSize, search ?? string.Empty);
            var totalCount = await _repository.GetTotalCountAsync(search ?? string.Empty);

            var response = new PaginatedResponse<BlockedCountry>
            {
                Items = countries,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }

        /// <summary>
        /// Temporarily block a country for a specific duration
        /// </summary>
        /// <param name="request">Country code and duration in minutes (1-1440)</param>
        /// <returns>Success message with expiration details</returns>
        /// <response code="200">Country successfully blocked temporarily</response>
        /// <response code="400">Invalid request or country code</response>
        /// <response code="409">Country is already blocked (permanently or temporarily)</response>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/countries/temporal-block
        ///     {
        ///        "countryCode": "EG",
        ///        "durationMinutes": 120
        ///     }
        ///
        /// The country will be automatically unblocked after the specified duration.
        /// A background service runs every 5 minutes to clean up expired blocks.
        /// </remarks>
        [HttpPost("temporal-block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> TemporalBlock([FromBody] TemporalBlockRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var countryCode = request.CountryCode.ToUpper();

            // Check if already blocked
            var existing = await _repository.GetBlockedCountryAsync(countryCode);
            if (existing != null)
            {
                if (existing.IsTemporary)
                {
                    return Conflict(new
                    {
                        message = $"Country {countryCode} is already temporarily blocked."
                    });
                }
                else
                {
                    return Conflict(new
                    {
                        message = $"Country {countryCode} is already permanently blocked."
                    });
                }
            }

            // Validate country code
            if (!CountryCodeValidator.IsValid(countryCode))
            {
                return BadRequest(new { message = $"Invalid country code: {countryCode}" });
            }

            var expiresAt = DateTime.UtcNow.AddMinutes(request.DurationMinutes);

            var blockedCountry = new BlockedCountry
            {
                CountryCode = countryCode,
                CountryName = CountryHelper.GetCountryName(countryCode),
                BlockedAt = DateTime.UtcNow,
                IsTemporary = true,
                DurationMinutes = request.DurationMinutes,
                ExpiresAt = expiresAt
            };

            var added = await _repository.AddBlockedCountryAsync(blockedCountry);

            if (!added)
            {
                return Conflict(new
                {
                    message = $"Failed to create temporal block for {countryCode}."
                });
            }

            _logger.LogInformation(
                "Country {CountryCode} temporarily blocked for {Duration} minutes until {ExpiresAt}",
                countryCode,
                request.DurationMinutes,
                expiresAt);

            return Ok(new
            {
                message = $"Country {countryCode} temporarily blocked for {request.DurationMinutes} minutes.",
                countryCode = blockedCountry.CountryCode,
                durationMinutes = blockedCountry.DurationMinutes,
                expiresAt = blockedCountry.ExpiresAt
            });
        }
    }
}

