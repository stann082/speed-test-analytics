using domain;
using Serilog;
using worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
    }).ConfigureLogging((hostContext, builder) =>
    {
        Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .WriteTo.NewRelicLogs(applicationName: "Speed Test", licenseKey: GetNewRelicApiKey())
                .CreateLogger();
    }).UseSerilog().Build();

string GetNewRelicApiKey()
{
    return EnvironmentVariableProvider.GetEnvironmentVariable("NewRelicApiKey");
}

await host.RunAsync();
