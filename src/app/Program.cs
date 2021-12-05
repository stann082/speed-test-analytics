using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                Console.WriteLine($"An un expected error has occurred...{Environment.NewLine}{ex}");
            }
        }

        #endregion

        #region Helper Methods

        public static IConfiguration LoadConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            IConfiguration config = LoadConfiguration();
            services.AddSingleton(config);
            services.AddTransient<IAwsDynamoDbService, AwsDynamoDbService>();
            services.AddTransient<IProcessService, ProcessService>();
            services.AddTransient<Application>();

            return services;
        }

        #endregion

    }
}
