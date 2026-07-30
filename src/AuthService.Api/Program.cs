using System.Reflection;
using AuthService.Api.Extensions;
using AuthService.Api.Middlewares;
using AuthService.Api.ModelBinders;
using AuthService.Persistence.Data;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// CONFIGURACIÓN
// Bypass SSL para servicios externos como Cloudinary.
System.Net.ServicePointManager.ServerCertificateValidationCallback +=
    (sender, certificate, chain, sslPolicyErrors) => true;

// Configurar Serilog como sistema principal de logging.
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

// Controladores y configuración JSON.
builder.Services
    .AddControllers(options =>
    {
        options.ModelBinderProviders.Insert(
            0,
            new FileDataModelBinderProvider()
        );
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// SERVICIOS DE LA APLICACIÓN
builder.Services.AddApiDocumentation();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRateLimitingPolicies();

// SEGURIDAD
builder.Services.AddSecurityPolicies(builder.Configuration);
builder.Services.AddSecurityOptions();

// SWAGGER
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath = Path.Combine(
        AppContext.BaseDirectory,
        xmlFile
    );

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// SWAGGER EN DESARROLLO
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// LOGGING DE PETICIONES
app.UseSerilogRequestLogging();

// HEADERS DE SEGURIDAD
app.UseSecurityHeaders(policies => policies
    .AddDefaultSecurityHeaders()
    .RemoveServerHeader()
    .AddFrameOptionsDeny()
    .AddXssProtectionBlock()
    .AddContentTypeOptionsNoSniff()
    .AddReferrerPolicyStrictOriginWhenCrossOrigin()
    .AddContentSecurityPolicy(policy =>
    {
        policy.AddDefaultSrc().Self();
        policy.AddScriptSrc().Self().UnsafeInline();
        policy.AddStyleSrc().Self().UnsafeInline();
        policy.AddImgSrc().Self().Data();
        policy.AddFontSrc().Self().Data();
        policy.AddConnectSrc().Self();
        policy.AddFrameAncestors().None();
        policy.AddBaseUri().Self();
        policy.AddFormAction().Self();
    })
    .AddCustomHeader(
        "Permissions-Policy",
        "geolocation=(), microphone=(), camera=()"
    )
    .AddCustomHeader(
        "Cache-Control",
        "no-store, no-cache, must-revalidate, private"
    )
);

// MANEJO GLOBAL DE ERRORES
app.UseMiddleware<GlobalExceptionMiddleware>();

// MIDDLEWARES PRINCIPALES
app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// CONTROLADORES
app.MapControllers();

// HEALTH CHECK PERSONALIZADO
app.MapGet("/health", () =>
{
    var response = new
    {
        status = "Healthy",
        timestamp = DateTime.UtcNow.ToString(
            "yyyy-MM-ddTHH:mm:ss.fffZ"
        )
    };

    return Results.Ok(response);
});

// HEALTH CHECK ESTÁNDAR
app.MapHealthChecks("/api/v1/health");

// LOG DE INICIO
var startupLogger =
    app.Services.GetRequiredService<ILogger<Program>>();

app.Lifetime.ApplicationStarted.Register(() =>
{
    try
    {
        var server =
            app.Services.GetRequiredService<IServer>();

        var addressesFeature =
            server.Features.Get<IServerAddressesFeature>();

        var addresses =
            (IEnumerable<string>?)addressesFeature?.Addresses
            ?? app.Urls;

        if (addresses != null && addresses.Any())
        {
            foreach (var address in addresses)
            {
                var healthUrl =
                    $"{address.TrimEnd('/')}/health";

                startupLogger.LogInformation(
                    "AuthService API is running at {Url}. Health endpoint: {HealthUrl}",
                    address,
                    healthUrl
                );
            }
        }
        else
        {
            startupLogger.LogInformation(
                "AuthService API started. Health endpoint: /health"
            );
        }
    }
    catch (Exception ex)
    {
        startupLogger.LogWarning(
            ex,
            "Failed to determine the listening addresses for startup log"
        );
    }
});

// INICIALIZAR BASE DE DATOS Y EJECUTAR SEEDER
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    var logger =
        scope.ServiceProvider
            .GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation(
            "Checking database connection..."
        );

        var canConnect =
            await context.Database.CanConnectAsync();

        if (!canConnect)
        {
            throw new InvalidOperationException(
                "Could not connect to the PostgreSQL database."
            );
        }

        logger.LogInformation(
            "Database connection successful."
        );

        logger.LogInformation(
            "Applying database migrations..."
        );

        await context.Database.MigrateAsync();

        logger.LogInformation(
            "Database migrations applied successfully."
        );

        logger.LogInformation(
            "Running seed data..."
        );

        await DataSeeder.SeedAsync(context);

        logger.LogInformation(
            "Database initialization completed successfully."
        );
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "An error occurred while initializing the database"
        );

        throw;
    }
}

app.Run();