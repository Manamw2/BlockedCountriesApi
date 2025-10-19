using BlockedCountriesApi.Helpers;
using BlockedCountriesApi.Models.Dtos;
using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using BlockedCountriesApi.Services.Interfaces;
using BlockedCountriesApi.Validators;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesApi.Controllers
{
    [ApiController]
    [Route("api/ip")]
    public class IpController : ControllerBase
    {
        private readonly IGeolocationService _geolocationService;
        private readonly IBlockedCountryRepository _countryRepository;
        private readonly IBlockedAttemptLogRepository _logRepository;
        private readonly ILogger<IpController> _logger;

        public IpController(
            IGeolocationService geolocationService,
            IBlockedCountryRepository countryRepository,
            IBlockedAttemptLogRepository logRepository,
            ILogger<IpController> logger)
        {
            _geolocationService = geolocationService;
            _countryRepository = countryRepository;
            _logRepository = logRepository;
            _logger = logger;
        }

        /// <summary>
        /// Lookup country information for an IP address
        /// </summary>
        /// <param name="ipAddress">IP address to lookup (optional - uses caller's IP if omitted)</param>
        /// <returns>Geolocation details including country, city, and coordinates</returns>
        /// <response code="200">Returns geolocation information</response>
        /// <response code="400">Invalid IP address format</response>
        /// <response code="500">Failed to fetch geolocation data</response>
        /// <remarks>
        /// If ipAddress parameter is not provided, the API will automatically detect
        /// and use the caller's IP address (supporting X-Forwarded-For headers).
        /// 
        /// Sample request:
        ///
        ///     GET /api/ip/lookup?ipAddress=8.8.8.8
        ///
        /// </remarks>
        [HttpGet("lookup")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LookupIp([FromQuery] string? ipAddress = null)
        {
            // If no IP provided, use caller's IP
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = HttpContextHelper.GetClientIpAddress(HttpContext);
                _logger.LogInformation("No IP provided, using caller's IP: {IpAddress}", ipAddress);
            }

            // Validate IP format
            if (!IpAddressValidator.IsValid(ipAddress))
            {
                return BadRequest(new { message = "Invalid IP address format." });
            }

            try
            {
                var geoData = await _geolocationService.GetCountryByIpAsync(ipAddress);

                var response = new IpLookupResponse
                {
                    IpAddress = geoData.Ip,
                    CountryCode = geoData.Location.CountryCode2,
                    CountryName = geoData.Location.CountryName,
                    City = geoData.Location.City,
                    StateProvince = geoData.Location.StateProv,
                    Isp = "N/A", // IPGeolocation.io requires premium plan for ISP
                    Latitude = geoData.Location.Latitude,
                    Longitude = geoData.Location.Longitude,
                    CountryFlag = geoData.Location.CountryFlag
                };

                _logger.LogInformation(
                    "IP lookup successful: {IpAddress} -> {CountryCode} ({CountryName})",
                    ipAddress,
                    response.CountryCode,
                    response.CountryName);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error looking up IP address: {IpAddress}", ipAddress);
                return StatusCode(500, new
                {
                    message = "Failed to lookup IP address.",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Check if the caller's IP is blocked based on country
        /// </summary>
        /// <returns>Block status with country information</returns>
        /// <response code="200">Access allowed - country is not blocked</response>
        /// <response code="403">Access denied - country is blocked</response>
        /// <response code="500">Failed to check block status</response>
        /// <remarks>
        /// This endpoint automatically:
        /// - Detects the caller's IP address (supports X-Forwarded-For headers)
        /// - Fetches country information via geolocation API
        /// - Checks if the country is blocked (permanent or temporal)
        /// - Logs the attempt with IP, country, timestamp, and user-agent
        /// 
        /// Sample response (blocked):
        ///
        ///     {
        ///       "ipAddress": "1.2.3.4",
        ///       "countryCode": "US",
        ///       "countryName": "United States",
        ///       "isBlocked": true,
        ///       "blockType": "Permanent",
        ///       "message": "Access denied. Your country (United States) is blocked (Permanent)."
        ///     }
        ///
        /// </remarks>
        [HttpGet("check-block")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CheckBlock()
        {
            var clientIp = HttpContextHelper.GetClientIpAddress(HttpContext);
            var userAgent = HttpContextHelper.GetUserAgent(HttpContext);

            _logger.LogInformation("Checking block status for IP: {IpAddress}", clientIp);

            try
            {
                // Fetch country from IP
                var geoData = await _geolocationService.GetCountryByIpAsync(clientIp);
                var countryCode = geoData.Location.CountryCode2;
                var countryName = geoData.Location.CountryName;

                // Check if country is blocked (permanent or temporal)
                var isBlocked = await _countryRepository.IsCountryBlockedAsync(countryCode);

                // Determine block type
                string? blockType = null;
                if (isBlocked)
                {
                    var permanentBlock = await _countryRepository.GetBlockedCountryAsync(countryCode);
                    if (permanentBlock != null)
                    {
                        blockType = "Permanent";
                    }
                    else if (await _countryRepository.IsTemporallyBlockedAsync(countryCode))
                    {
                        blockType = "Temporal";
                    }
                }

                // Log the attempt
                var log = new BlockedAttemptLog
                {
                    IpAddress = clientIp,
                    CountryCode = countryCode,
                    IsBlocked = isBlocked,
                    UserAgent = userAgent,
                    Timestamp = DateTime.UtcNow
                };

                await _logRepository.AddLogAsync(log);

                // Build response
                var response = new BlockCheckResponse
                {
                    IpAddress = clientIp,
                    CountryCode = countryCode,
                    CountryName = countryName,
                    IsBlocked = isBlocked,
                    BlockType = blockType ?? string.Empty,
                    Message = isBlocked
                        ? $"Access denied. Your country ({countryName}) is blocked ({blockType})."
                        : $"Access allowed. Your country ({countryName}) is not blocked."
                };

                _logger.LogInformation(
                    "Block check: {IpAddress} from {CountryCode} - Blocked: {IsBlocked}",
                    clientIp,
                    countryCode,
                    isBlocked);

                // Return 403 if blocked, 200 if allowed
                if (isBlocked)
                {
                    return StatusCode(403, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking block status for IP: {IpAddress}", clientIp);
                return StatusCode(500, new
                {
                    message = "Failed to check block status.",
                    error = ex.Message
                });
            }
        }

        
    }
}

