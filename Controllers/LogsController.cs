using BlockedCountriesApi.Models.Dtos;
using BlockedCountriesApi.Models.Entities;
using BlockedCountriesApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlockedCountriesApi.Controllers
{
    [ApiController]
    [Route("api/logs")]
    public class LogsController : ControllerBase
    {
        private readonly IBlockedAttemptLogRepository _logRepository;
        private readonly ILogger<LogsController> _logger;

        public LogsController(
            IBlockedAttemptLogRepository logRepository,
            ILogger<LogsController> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of blocked access attempts
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, max: 100)</param>
        /// <returns>Paginated list of access attempt logs</returns>
        /// <response code="200">Returns the paginated list of logs</response>
        /// <response code="400">Invalid pagination parameters</response>
        /// <response code="500">Failed to retrieve logs</response>
        /// <remarks>
        /// Returns logs with the following information:
        /// - IP address of the request
        /// - Country code detected
        /// - Timestamp of the attempt
        /// - Whether the access was blocked (true/false)
        /// - User-Agent string from the request
        /// 
        /// Logs are sorted by timestamp (newest first).
        /// </remarks>
        [HttpGet("blocked-attempts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBlockedAttempts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1)
            {
                return BadRequest(new { message = "Page must be greater than 0." });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Page size must be between 1 and 100." });
            }

            try
            {
                var logs = await _logRepository.GetLogsAsync(page, pageSize);
                var totalCount = await _logRepository.GetTotalCountAsync();

                var response = new PaginatedResponse<BlockedAttemptLog>
                {
                    Items = logs,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation(
                    "Retrieved {Count} blocked attempt logs (Page {Page} of {TotalPages})",
                    logs.Count,
                    page,
                    response.TotalPages);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blocked attempt logs");
                return StatusCode(500, new
                {
                    message = "Failed to retrieve blocked attempt logs.",
                    error = ex.Message
                });
            }
        }
    }
}

