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
// Swagger/OpenAPI con Swashbuckle (genera schemas completos)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

        // PASO 1: Clasificar el tipo de error
        var (statusCode, title, detail, isExpectedError) = exception switch
        {
            // ERRORES ESPERADOS (del negocio/cliente) - OK mostrar detalles
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Not Found", notFound.Message, true),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Invalid API credentials", true),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message, true),
            BadRequestException badRequest => (StatusCodes.Status400BadRequest, "Bad Request", badRequest.Message, true),
            
            // ERROR DE API EXTERNA (Dolibarr) - Mostrar que es externo pero no detalles internos
            ApiException apiEx => (StatusCodes.Status500InternalServerError, "External Service Error", 
                env.IsDevelopment() ? apiEx.Message : "The external service is temporarily unavailable", false),
            
            // ERRORES INESPERADOS (bugs del programador) 
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", 
                env.IsDevelopment() 
                    ? exception?.Message ?? "An unexpected error occurred"  // DESARROLLO: muestra el error real
                    : "An unexpected error occurred. Please try again later.", // PRODUCCIÓN: mensaje genérico
                false)
        };

        // PASO 2: SIEMPRE loguear (crítico para debugging en producción)
        if (isExpectedError)
        {
            // Errores esperados: log como Warning (no son bugs, son flujo normal)
            logger.LogWarning(exception, 
                "Expected error: {ExceptionType} | Path: {Path} | Message: {Message}",
                exception?.GetType().Name, context.Request.Path, exception?.Message);
        }
        else
        {
            // Errores inesperados: log como Error (son bugs que HAY QUE ARREGLAR)
            logger.LogError(exception, 
                "UNHANDLED EXCEPTION: {ExceptionType} | Path: {Path} | Message: {Message} | StackTrace: {StackTrace}",
                exception?.GetType().Name, context.Request.Path, exception?.Message, exception?.StackTrace);
        }

        // PASO 3: Construir respuesta ProblemDetails (estándar RFC 7807)
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        // PASO 4: EN DESARROLLO añadir info extra para debugging
        if (env.IsDevelopment() && !isExpectedError)
        {
            problemDetails.Extensions["exceptionType"] = exception?.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception?.StackTrace;
            if (exception?.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = exception.InnerException.Message;
                problemDetails.Extensions["innerStackTrace"] = exception.InnerException.StackTrace;
            }
        }

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DoliMiddlewareApi v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();