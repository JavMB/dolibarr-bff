# AGENTS.md - Development Guidelines for DoliMiddlewareApi

This file contains essential information for AI coding agents working on the DoliMiddlewareApi project. Follow these guidelines to maintain consistency and quality.

## Project Overview
DoliMiddlewareApi is a C# ASP.NET Core Web API (targeting .NET 10.0) that serves as a middleware between client applications and Dolibarr ERP system. It provides invoice management functionality with RESTful endpoints.

## Build/Lint/Test Commands

### Building
```bash
# Build in Debug mode (default)
dotnet build

# Build in Release mode
dotnet build -c Release

# Clean and build
dotnet clean && dotnet build
```

### Running
```bash
# Run the application (includes hot reload in development)
dotnet run

# Run with specific configuration
dotnet run --environment Development
```

### Linting/Formatting
```bash
# Format code according to project standards
dotnet format

# Check formatting without making changes
dotnet format --verify-no-changes

# Format whitespace only
dotnet format whitespace
```

### Testing
**Note**: This project currently has no test framework configured. When adding tests:

```bash
# Add xUnit test framework (recommended)
dotnet add package xunit
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package xunit.runner.visualstudio

# Run tests (after setup)
dotnet test

# Run tests with coverage (after adding coverlet.collector)
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test
dotnet test --filter "TestMethodName"

# Run tests in a specific class
dotnet test --filter "ClassName"

# Run tests in watch mode
dotnet watch test
```

### Docker
```bash
# Build Docker image
docker build -t dolimiddlewareapi .

# Run Docker container
docker run -p 8080:8080 dolimiddlewareapi

# Run with environment variables
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development dolimiddlewareapi
```

## Code Style Guidelines

### Language Features & Project Setup
- **Target Framework**: .NET 10.0
- **Nullable Reference Types**: Enabled - use `?` for nullable types, avoid `!` suppressions
- **Implicit Usings**: Enabled - core namespaces are automatically imported
- **Top-level Statements**: Not used (traditional Program.cs structure)
- **Authentication**: JWT Bearer tokens with session-based caching

### Naming Conventions
- **Classes**: PascalCase (e.g., `InvoiceService`, `CreateInvoiceDto`)
- **Methods**: PascalCase (e.g., `GetInvoiceAsync`, `MapToInvoiceDto`)
- **Properties**: PascalCase (e.g., `ClientId`, `TotalAmount`)
- **Private Fields**: camelCase with underscore prefix (e.g., `_httpClient`, `_invoiceService`)
- **Constants**: PascalCase (e.g., `DefaultPageSize`)
- **Namespaces**: Follow folder structure (e.g., `DoliMiddlewareApi.Controllers`)
- **Files**: Match class name (e.g., `InvoiceService.cs`)

### Imports & Using Statements
```csharp
// Group usings by:
// 1. System namespaces
// 2. Microsoft namespaces
// 3. Third-party packages
// 4. Project namespaces
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using DoliMiddlewareApi.Dtos;
using DoliMiddlewareApi.Services;

// Remove unused usings automatically with: dotnet format
```

### Formatting & Structure
- **Indentation**: 4 spaces (follow .editorconfig when created)
- **Braces**: K&R style (opening brace on same line)
- **Line Length**: Aim for 100-120 characters, break long lines appropriately
- **File Structure**: One class per file, except for small related classes

### Controller Guidelines
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT authentication required
public class InvoicesController : ControllerBase
{
    private readonly InvoiceService _invoiceService;

    // Constructor injection only
    public InvoicesController(InvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    // Use ProducesResponseType for all endpoints
    [HttpGet]
    [ProducesResponseType(typeof(List<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices() => Ok(await _invoiceService.GetInvoicesAsync());
}
```

### Service Layer Guidelines
```csharp
public class InvoiceService
{
    private readonly DolibarrApiClient _apiClient;

    public InvoiceService(DolibarrApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Async all the way - use Task for all public methods
    public async Task<InvoiceDetailDto> GetInvoiceAsync(int id)
    {
        var data = await _apiClient.GetResourceAsync<InvoiceDetailResponse>($"invoices/{id}");
        return InvoiceMapper.MapToInvoiceDetailDto(data);
    }
}
```

### DTO Guidelines
```csharp
// Command DTOs (input)
public class CreateInvoiceDto
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    // Use meaningful defaults for optional properties
    public string Status { get; set; } = "draft";

    [Required]
    [MinLength(1)]
    public List<CreateInvoiceLineDto> Lines { get; set; } = new();
}

// Query DTOs (output) - handle nulls properly with nullable enabled
public class InvoiceDto
{
    public int Id { get; set; }
    public string Number { get; set; } = ""; // Provide defaults for non-nullable properties
    public DateTime? Date { get; set; }
    public string Status { get; set; } = "";
}
```

### Error Handling
```csharp
// Custom exceptions for business logic
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// Use specific exceptions in services
if (invoice == null)
    throw new NotFoundException($"Invoice with id {id} not found");

// Global error handling in Program.cs provides consistent API responses
// Logs errors appropriately based on type (expected vs unexpected)
```

### HTTP Client Usage
```csharp
// Use typed clients registered in DI
builder.Services.AddHttpClient<DolibarrApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Dolibarr:ApiUrl"]!);
    client.DefaultRequestHeaders.Add("DOLAPIKEY", builder.Configuration["Dolibarr:ApiKey"]!);
});

// Generic methods for common operations
public async Task<T> GetResourceAsync<T>(string endpoint) where T : class
{
    var response = await _httpClient.GetAsync(endpoint);
    await EnsureSuccessOrThrowAsync(response, endpoint);
    return await response.Content.ReadFromJsonAsync<T>()
           ?? throw new ApiException($"Failed to deserialize response from Dolibarr for endpoint '{endpoint}'");
}
```

### JSON Serialization
```csharp
// Configure in Program.cs for consistency
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums as strings
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // camelCase properties
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
```

### Mapping Guidelines
```csharp
// Static mapper classes
public static class InvoiceMapper
{
    public static InvoiceDto MapToInvoiceDto(InvoiceResponse response)
    {
        return new InvoiceDto
        {
            Id = int.TryParse(response.id, out int id) ? id : 0,
            Number = response.@ref ?? "SIN-REF",
            // Handle parsing failures gracefully
            Total = decimal.TryParse(response.total_ttc, NumberStyles.Any, CultureInfo.InvariantCulture,
                     out decimal total) ? Math.Round(total, 2) : null,
            Status = ConvertStatusToWord(response.statut)
        };
    }

    // Private helper methods for complex conversions
    private static string ConvertStatusToWord(string? statusNumber)
    {
        return statusNumber switch
        {
            "0" => "draft",
            "1" => "unpaid",
            "2" => "paid",
            "3" => "cancelled",
            _ => "unknown"
        };
    }
}
```

### Configuration Management
```csharp
// Strongly typed configuration classes
public class DolibarrSettings
{
    public string ApiUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
}

// Register in Program.cs
builder.Services.Configure<DolibarrSettings>(builder.Configuration.GetSection("Dolibarr"));
```

### Security Best Practices
- Never log sensitive data (API keys, passwords)
- Use proper validation attributes on DTOs
- Validate all input parameters
- Use HTTPS in production
- Implement proper authentication/authorization when needed

### Performance Considerations
- Use async/await throughout the stack
- Avoid blocking calls in async methods
- Use dependency injection for proper lifetime management
- Consider response caching for read operations
- Use pagination for list endpoints

### Logging Guidelines
```csharp
// Use structured logging with semantic parameters
_logger.LogInformation("Processing invoice creation for client {ClientId}", clientId);

// Log levels:
// - Information: Normal operations
// - Warning: Expected errors (business logic failures)
// - Error: Unexpected errors (bugs)
// - Debug: Detailed debugging information (development only)
```

## Development Workflow

1. **Before making changes**: Run `dotnet build` to ensure current state compiles
2. **Make changes**: Follow the established patterns and conventions
3. **Format code**: Run `dotnet format` to ensure consistent formatting
4. **Test changes**: Run the application and test endpoints manually
5. **Build verification**: Run `dotnet build -c Release` before committing

## Architecture Patterns Used

- **Clean Architecture**: Controllers → Services → External APIs
- **Dependency Injection**: Constructor injection throughout
- **Repository Pattern**: Not implemented (direct API calls)
- **CQRS**: Separated command/query DTOs
- **Mapper Pattern**: Static mappers for data transformation
- **Exception Handling**: Global error handling middleware
- **Service Layer Separation**: Each service has a single responsibility (SRP)
  - `DolibarrTokenCacheService`: Token caching (no HTTP calls)
  - `DolibarrApiClient`: HTTP calls only (no cache logic)
  - `InvoiceService`: Business logic (uses cached tokens via HTTP client)
  - `AuthApplicationService`: Orchestrates login + token caching
  - `DolibarrAuthService`: Authenticates with external API

## Future Considerations

When adding tests:
- Use xUnit as the testing framework
- Add integration tests for API endpoints
- Mock external API calls using Moq or NSubstitute
- Aim for high test coverage on business logic

When adding authentication:
- JWT token validation with Microsoft.AspNetCore.Authentication.JwtBearer
- Session-based caching for Dolibarr API tokens (handled in DolibarrTokenCacheService)
- [Authorize] attributes on protected controllers

When scaling:
- Consider adding response caching
- Implement rate limiting
- Add request/response logging middleware
- Consider API versioning strategy</content>
<parameter name="filePath">AGENTS.md