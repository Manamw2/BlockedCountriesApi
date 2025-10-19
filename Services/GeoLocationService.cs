using BlockedCountriesApi.Models.External;
using BlockedCountriesApi.Services.Interfaces;
using Newtonsoft.Json;

namespace BlockedCountriesApi.Services
{
    public class GeolocationService : IGeolocationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeolocationService> _logger;
        private readonly string _apiKey;

        public GeolocationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeolocationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;

            _apiKey = _configuration["GeolocationApi:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("Geolocation API key is not configured in appsettings.json");
            }

            _httpClient.BaseAddress = new Uri(_configuration["GeolocationApi:BaseUrl"]);
            _httpClient.Timeout = TimeSpan.FromSeconds(
                _configuration.GetValue<int>("GeolocationApi:TimeoutSeconds", 10)
            );
        }

        public async Task<IPGeolocationResponse> GetCountryByIpAsync(string ipAddress)
        {
            try
            {
                // Build the request URL
                var url = $"v2/ipgeo?apiKey={_apiKey}&ip={ipAddress}";

                _logger.LogInformation("Calling IPGeolocation.io API for IP: {IpAddress}", ipAddress);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("IPGeolocation API returned {StatusCode}: {Error}",
                        response.StatusCode, errorContent);
                    throw new HttpRequestException($"Geolocation API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<IPGeolocationResponse>(content);

                if (result == null || result.Location == null)
                {
                    throw new InvalidOperationException("Invalid response from Geolocation API");
                }

                _logger.LogInformation("Successfully retrieved geolocation for {IpAddress}: {Country}",
                    ipAddress, result.Location.CountryName);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling geolocation API for IP: {IpAddress}", ipAddress);
                throw new Exception($"Failed to fetch geolocation data: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for IP: {IpAddress}", ipAddress);
                throw new Exception("Failed to parse geolocation response", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling geolocation API for IP: {IpAddress}", ipAddress);
                throw;
            }
        }
    }
}
