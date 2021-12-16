using domain;
using Serilog;
using service;
using worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        IConfiguration config = LoadConfiguration();
        services.AddHostedService<Worker>()
                .AddTransient<IAwsDynamoDbService, AwsDynamoDbService>()
                .AddTransient<IProcessService, ProcessService>();
    }).ConfigureLogging((hostContext, builder) =>
    {
        Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .WriteTo.NewRelicLogs(applicationName: "Speed Test", licenseKey: GetNewRelicApiKey())
                .CreateLogger();
    }).UseSerilog().Build();

IConfiguration LoadConfiguration()
{
    IConfigurationBuilder builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    return builder.Build();
}

string GetNewRelicApiKey()
{
    return EnvironmentVariableProvider.GetEnvironmentVariable("NewRelicApiKey");
}

await host.RunAsync();

