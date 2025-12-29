using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Services;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializar enums como strings en vez de números
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());

        // Usar camelCase para propiedades (id en vez de Id)
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// Typed Client - Inyecta HttpClient directamente en DolibarrApiClient
builder.Services.AddHttpClient<DolibarrApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Dolibarr:ApiUrl"]!);
    client.DefaultRequestHeaders.Add("DOLAPIKEY", builder.Configuration["Dolibarr:ApiKey"]!);
});

var app = builder.Build();

// Global exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandler?.Error;

        var (statusCode, message) = exception switch
        {
            NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message),
            UnauthorizedException unauthorized => (StatusCodes.Status401Unauthorized, unauthorized.Message),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, forbidden.Message),
            BadRequestException badRequest => (StatusCodes.Status400BadRequest, badRequest.Message),
            ApiException apiEx => (StatusCodes.Status500InternalServerError, apiEx.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();