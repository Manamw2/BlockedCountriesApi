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
| GET | `/api/countries/blocked` | Get all blocked countries - both permanent and temporal (paginated, searchable) |
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

### Get Blocked Countries (includes both permanent and temporal)
```bash
GET /api/countries/blocked?page=1&pageSize=10
```

**Response Example:**
```json
{
  "items": [
    {
      "countryCode": "US",
      "countryName": "United States",
      "blockedAt": "2025-10-19T10:00:00Z",
      "isTemporary": false,
      "durationMinutes": null,
      "expiresAt": null
    },
    {
      "countryCode": "EG",
      "countryName": "Egypt",
      "blockedAt": "2025-10-19T12:00:00Z",
      "isTemporary": true,
      "durationMinutes": 120,
      "expiresAt": "2025-10-19T14:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 2
}
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

### Unified Blocked Countries Model
Both permanent and temporal blocks are managed in a single unified list. Each blocked country entry includes:
- `countryCode`: ISO 2-letter country code
- `countryName`: Full country name
- `blockedAt`: Timestamp when the block was created
- `isTemporary`: `true` for temporal blocks, `false` for permanent blocks
- `durationMinutes`: Duration in minutes for temporal blocks (null for permanent)
- `expiresAt`: Expiration timestamp for temporal blocks (null for permanent)

This unified approach ensures that:
- The `/api/countries/blocked` endpoint returns **both** permanent and temporal blocks
- You can easily identify block type and remaining time
- Temporal blocks can be upgraded to permanent blocks

### In-Memory Storage
All data is stored in memory using thread-safe collections:
- `ConcurrentDictionary` for blocked countries (both permanent and temporal)
- `ConcurrentBag` for access logs

### Background Service
A background service runs every 5 minutes to automatically remove expired temporal blocks.

### IP Detection
Automatically detects caller's IP address with support for:
- X-Forwarded-For header
- X-Real-IP header
- Direct connection IP

### Temporal to Permanent Upgrade
Temporarily blocked countries can be upgraded to permanent blocks seamlessly by calling the permanent block endpoint. This will:
- Set `isTemporary` to `false`
- Clear the `durationMinutes` and `expiresAt` fields
- Keep the country in the blocked list indefinitely

## Notes

- Data is not persisted between application restarts
- The API uses IPGeolocation.io for geolocation services
- Background cleanup service runs every 5 minutes


