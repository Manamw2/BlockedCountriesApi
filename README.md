# Blocked Countries API

A .NET 9 Web API for managing blocked countries and validating IP addresses using geolocation services.

## Features

- Block countries permanently or temporarily
- IP address geolocation lookup
- Automatic IP-based country blocking validation
- Access attempt logging
- Background service for temporal block cleanup
- In-memory data storage
- Swagger/OpenAPI documentation

## Technologies

- .NET 9
- ASP.NET Core Web API
- IPGeolocation.io API
- Swashbuckle (Swagger)
- In-memory collections (ConcurrentDictionary, ConcurrentBag)

## Prerequisites

- .NET 9 SDK
- IPGeolocation.io API Key (free tier available)

## Configuration

Update the API key in `appsettings.json`:

```json
{
  "GeolocationApi": {
    "BaseUrl": "https://api.ipgeolocation.io/",
    "ApiKey": "your-api-key-here",
    "TimeoutSeconds": 10
  }
}
```

## How to Run

### Using Command Line
```bash
cd BlockedCountriesApi
dotnet restore
dotnet build
dotnet run
```

Then navigate to: `https://localhost:7159/swagger`

### Or use Watch Mode (Auto-opens browser)
```bash
cd BlockedCountriesApi
dotnet watch run
```
This will automatically open the Swagger UI in your default browser.

The API will be available at:
- HTTPS: `https://localhost:7159`
- Swagger UI: `https://localhost:7159/swagger`

## API Endpoints

### Countries Management

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/countries/block` | Add a country to permanent block list |
| DELETE | `/api/countries/block/{countryCode}` | Remove a country from block list |
| GET | `/api/countries/blocked` | Get all blocked countries (paginated, searchable) |
| POST | `/api/countries/temporal-block` | Temporarily block a country (1-1440 minutes) |

### IP Validation

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/ip/lookup` | Lookup country info for an IP address |
| GET | `/api/ip/check-block` | Check if caller's IP is blocked |

### Logging

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/logs/blocked-attempts` | Get paginated access attempt logs |

## Request Examples

### Block a Country
```bash
POST /api/countries/block
Content-Type: application/json

{
  "countryCode": "US"
}
```

### Temporarily Block a Country
```bash
POST /api/countries/temporal-block
Content-Type: application/json

{
  "countryCode": "EG",
  "durationMinutes": 120
}
```

### Lookup IP Address
```bash
GET /api/ip/lookup?ipAddress=8.8.8.8
```

### Check Your IP Status
```bash
GET /api/ip/check-block
```

## Project Structure

```
BlockedCountriesApi/
├── Controllers/          # API endpoints
├── Models/
│   ├── Dtos/            # Data transfer objects
│   ├── Entities/        # Domain entities
│   └── External/        # External API models
├── Repositories/        # Data access layer
├── Services/            # Business logic
├── Validators/          # Input validation
├── Helpers/             # Utility classes
└── Program.cs           # Application entry point
```

## Key Features

### In-Memory Storage
All data is stored in memory using thread-safe collections:
- `ConcurrentDictionary` for blocked countries
- `ConcurrentBag` for access logs

### Background Service
A background service runs every 5 minutes to automatically remove expired temporal blocks.

### IP Detection
Automatically detects caller's IP address with support for:
- X-Forwarded-For header
- X-Real-IP header
- Direct connection IP

### Temporal to Permanent Upgrade
Temporarily blocked countries can be upgraded to permanent blocks seamlessly.

## Notes

- Data is not persisted between application restarts
- The API uses IPGeolocation.io for geolocation services
- Background cleanup service runs every 5 minutes


