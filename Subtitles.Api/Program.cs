using Serilog;
using Microsoft.EntityFrameworkCore;
using Subtitles.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure multiple configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true)
    .AddEnvironmentVariables()
    .AddKeyPerFile("/run/secrets", optional: true); // Docker Swarm secrets

// Bootstrap logger for startup issues
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up application");

    // Configure Serilog from configuration after all config sources are loaded
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();

    Log.Information("Logger configured from settings");

    // Use Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    
    // Add Entity Framework and PostgreSQL
    builder.Services.AddDbContext<SubtitlesDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("Subtitles.Data");
                npgsqlOptions.CommandTimeout(30);
            }));

    var app = builder.Build();
    
    // Ensure database is created and migrated
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SubtitlesDbContext>();
        try
        {
            context.Database.Migrate();
            Log.Information("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the database");
            throw;
        }
    }

    // Configure the HTTP request pipeline.
    app.UseSerilogRequestLogging(); // Add request logging

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting the application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
