using FileIndexerService;
using Serilog;
using Serilog.Events;
using System.Reflection;

Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File("./logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.EventLog(source: "FileIndexerService", restrictedToMinimumLevel: LogEventLevel.Information)
    .CreateLogger();

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureLogging(loggerFactory => loggerFactory.AddEventLog())
    .ConfigureAppConfiguration((hostContext, configBuilder) =>
    {
        configBuilder
            .AddJsonFile("appsettings.json")
            .Build();
    })
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext())
    .ConfigureServices(services =>
    {
        services.AddLogging();
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
