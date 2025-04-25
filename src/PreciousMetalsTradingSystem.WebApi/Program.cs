using PreciousMetalsTradingSystem.Domain;
using PreciousMetalsTradingSystem.Application;
using PreciousMetalsTradingSystem.Infrastructure;
using PreciousMetalsTradingSystem.Infrastructure.Database;
using PreciousMetalsTradingSystem.Infrastructure.Jobs;
using PreciousMetalsTradingSystem.WebApi;
using Serilog;
using Serilog.Events;

Serilog.Debugging.SelfLog.Enable(Console.Error);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] (TraceID: {TraceID}) {Message}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.AddSerilog();
    
    bool initializeForTests = args.Contains("--integrationTestsRunning=true");

    builder.Services
        .AddDomain()
        .AddApplication()
        .AddInfrastructure(builder.Configuration, initializeForTests)
        .AddWebServices(builder.Configuration);

    var app = builder.Build();


    await DatabaseInitializer.InitializeDatabase(app, initializeForTests);
    JobInitializer.InitializeJobs(app, initializeForTests);

    app.UseWebServices();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Startup failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
