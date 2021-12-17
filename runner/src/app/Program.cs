using domain;
using Serilog;
using service;

namespace worker;

public class Program
{

    #region Public Methods

    public static async Task Main(string[] args)
    {
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

        await host.RunAsync();
    }

    #endregion

    #region Helper Methods

    private static IConfiguration LoadConfiguration()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        return builder.Build();
    }

    private static string GetNewRelicApiKey()
    {
        return EnvironmentVariableProvider.GetEnvironmentVariable("NewRelicApiKey");
    }

    #endregion

}



