using service;
using System;
using System.IO;
using System.Linq;

namespace speed_test
{
    public class Application
    {

        #region Constants

        private const string TABLE_NAME = "SpeedTestAnalytics";

        #endregion

        #region Constructors

        public Application(IAwsDynamoDbService dynamoDbService)
        {
            DynamoDbService = dynamoDbService;
        }

        #endregion

        #region Properties

        private IAwsDynamoDbService DynamoDbService { get; set; }

        #endregion

        #region Application Starting Point

        public void Run()
        {
            string speedTestFile = "speedtest.exe";
            if (!File.Exists(speedTestFile))
            {
                return;
            }

            DynamoDbService.CreateTable(TABLE_NAME);

            WaitForTableToActivate(DynamoDbService);

            string arguments = "--format=json";
            if (args.Length > 0)
            {
                string formatArgument = args.FirstOrDefault(a => a.Contains("--format"));
                if (!string.IsNullOrEmpty(formatArgument))
                {
                    arguments = formatArgument;
                }
            }

            string machineId = $"{Environment.MachineName} ({Environment.OSVersion.Platform})";
            Console.WriteLine($"Running a speed test for {machineId}...");
            ProcessRunnerService processService = new ProcessRunnerService(speedTestFile, arguments);
            processService.Run();
            if (string.IsNullOrEmpty(processService.StandardOutput))
            {
                Console.WriteLine("Speed test process did not yield any results... Please check logs for details.");
                return;
            }

            Console.WriteLine($"Adding speed test result entry to the {TABLE_NAME} table...");
            DynamoDbService.PutItem(TABLE_NAME, machineId, processService.StandardOutput);
            Console.WriteLine("An entry had been successfully added!");
        }

        #endregion

        #region Helper Methods

        private void WaitForTableToActivate(IAwsDynamoDbService service)
        {
            while (service.DescribeTable(TABLE_NAME).TableStatus != TableStatus.ACTIVE)
            {
                Console.WriteLine($"Waiting for the table {TABLE_NAME} to activate...");
                Thread.Sleep(5000);
            }
        }

        #endregion

    }
}
