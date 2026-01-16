using DoliMiddlewareApi.Exceptions;
using DoliMiddlewareApi.Services;
using DoliMiddlewareApi.Services.Clients;
using DoliMiddlewareApi.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Diagnostics;

// =========================================
// PROGRAM.CS - CONFIGURACIÓN Y DEPENDENCIAS
// =========================================

var builder = WebApplication.CreateBuilder(args);

if (string.IsNullOrEmpty(builder.Configuration["Jwt:Secret"]))
    throw new InvalidOperationException("JWT Secret is required in configuration. Set 'Jwt:Secret' in appsettings.json or environment variables.");

// =========================================
// 1. CONFIGURACIÓN DE SERVICIOS ASP.NET CORE
// =========================================

// Controllers + JSON (camelCase + enums as strings)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// JWT Standard (ASP.NET auto-validation)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DoliMiddleware",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DoliClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

 builder.Services.AddAuthorization();

 // CORS Configuration
 builder.Services.AddCors(options =>
 {
     options.AddPolicy("AllowVueApp",
         policy => policy
             .WithOrigins("http://localhost:3001", "http://localhost:3000")
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials());
 });

 // Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// 2. DOLIBARR HTTP CLIENT
// =========================================
// HttpClient named para Dolibarr (configurado solo con BaseAddress)
// DolibarrApiClient se crea manual porque necesita HttpClient + TokenCacheService
//
// POR QUÉ NO usar AddHttpClient<TClient>() directamente?
// --------------------------------------------------------
// HttpClient es COMPARTIDO entre todos los usuarios
// Si configuramos el token ahí, el Usuario A usaría el token del Usuario B
// Solución: TokenCacheService obtiene el token del USUARIO ACTUAL (del JWT)
// factory.CreateClient("Dolibarr") devuelve el MISMO HttpClient reusado
// No se crea un cliente por request, los headers son por request (thread-safe)
// =========================================

builder.Services.AddHttpClient("Dolibarr", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Dolibarr:ApiUrl"]!);
});

builder.Services.AddScoped<IDolibarrApiClient>(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var client = factory.CreateClient("Dolibarr");
    var tokenCacheService = sp.GetRequiredService<DolibarrTokenCacheService>();
    return new DolibarrApiClient(client, tokenCacheService);
});

// =========================================
// 3. REGISTRO DE SERVICIOS (DEPENDENCIAS)
// =========================================

// Servicio de negocio (facturas)
builder.Services.AddScoped<InvoiceService>();

// Servicio de aplicación (orquesta login + cache)
builder.Services.AddScoped<AuthApplicationService>();

// Servicio de autenticación con Dolibarr
builder.Services.AddScoped<DolibarrAuthService>();

// Servicio de cache de tokens de Dolibarr
builder.Services.AddScoped<DolibarrTokenCacheService>();

// Generador de JWT - Singleton porque es stateless
builder.Services.AddSingleton<JwtTokenProvider>();

// Cache en memoria para tokens (IMemoryCache)
builder.Services.AddMemoryCache();

// HttpContext accessor para acceder a User.Claims en servicios
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// =========================================
// 4. GLOBAL EXCEPTION HANDLER
// =========================================

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandler?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

        // Clasificar el tipo de error
        var (statusCode, title, detail, isExpectedError) = exception switch
        {
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Not Found", notFound.Message, true),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", "Invalid API credentials", true),
            ForbiddenException forbidden => (StatusCodes.Status403Forbidden, "Forbidden", forbidden.Message, true),
            BadRequestException badRequest => (StatusCodes.Status400BadRequest, "Bad Request", badRequest.Message, true),
            ApiException apiEx => (StatusCodes.Status500InternalServerError, "External Service Error",
                env.IsDevelopment() ? apiEx.Message : "The external service is temporarily unavailable", false),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error",
                env.IsDevelopment()
                    ? exception?.Message ?? "An unexpected error occurred"
                    : "An unexpected error occurred. Please try again later.", false)
        };

        // Loguear siempre (crítico para debugging)
        if (isExpectedError)
        {
            logger.LogWarning(exception,
                "Expected error: {ExceptionType} | Path: {Path} | Message: {Message}",
                exception?.GetType().Name, context.Request.Path, exception?.Message);
        }
        else
        {
            logger.LogError(exception,
                "UNHANDLED EXCEPTION: {ExceptionType} | Path: {Path} | Message: {Message} | StackTrace: {StackTrace}",
                exception?.GetType().Name, context.Request.Path, exception?.Message, exception?.StackTrace);
        }

        // Construir respuesta ProblemDetails (RFC 7807)
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        // En desarrollo añadir info extra para debugging
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

// =========================================
// 5. PIPELINE ASP.NET CORE
// =========================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DoliMiddlewareApi v1");
    });
}

app.UseHttpsRedirection();


app.UseCors("AllowVueApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
