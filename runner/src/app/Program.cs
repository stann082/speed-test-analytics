using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using service;
using System;
using System.IO;

namespace speed_test
{
    public class Program
    {

        #region Main Method

        public static void Main(string[] args)
        {
            try
            {
                IServiceCollection services = ConfigureServices();
                ServiceProvider serviceProvider = services.BuildServiceProvider();
                serviceProvider.GetService<Application>().Run();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "An un expected error has occurred...");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        #endregion

        #region Helper Methods

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            IConfiguration config = LoadConfiguration();

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.ReadFrom.Configuration(config);
            loggerConfiguration.WriteTo.NewRelicLogs(applicationName: "Speed Test", licenseKey: GetNewRelicApiKey());
            Log.Logger = loggerConfiguration.CreateLogger();

            services.AddSingleton(config);
            services.AddTransient<IAwsDynamoDbService, AwsDynamoDbService>();
            services.AddTransient<IProcessService, ProcessService>();
            services.AddTransient<Application>();

            return services;
        }

        private static string GetNewRelicApiKey()
        {
            return EnvironmentVariableProvider.GetEnvironmentVariable("NewRelicApiKey");
        }

        private static IConfiguration LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        #endregion

    }
}
