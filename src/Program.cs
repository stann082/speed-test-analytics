using Amazon.DynamoDBv2;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace speed_test
{
    public class Program
    {

        #region Constants

        private const string TABLE_NAME = "SpeedTestAnalytics";

        #endregion

        #region Main Method

        public static void Main(string[] args)
        {
            string speedTestFile = "speedtest.exe";
            if (!File.Exists(speedTestFile))
            {
                return;
            }

            IAwsDynamoDbClient client = new AwsDynamoDbClient();
            client.CreateTable(TABLE_NAME);

            WaitForTableToActivate(client);

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
            ProcessRunner process = new ProcessRunner(speedTestFile, arguments);
            process.Run();
            if (string.IsNullOrEmpty(process.StandardOutput))
            {
                Console.WriteLine("Speed test process did not yield any results... Please check logs for details.");
                return;
            }

            Console.WriteLine($"Adding speed test result entry to the {TABLE_NAME} table...");
            client.PutItem(TABLE_NAME, machineId, process.StandardOutput);
            Console.WriteLine("An entry had been successfully added!");
        }

        #endregion

        #region Helper Methods

        private static void WaitForTableToActivate(IAwsDynamoDbClient client)
        {
            while (client.DescribeTable(TABLE_NAME).TableStatus != TableStatus.ACTIVE)
            {
                Console.WriteLine($"Waiting for the table {TABLE_NAME} to activate...");
                Thread.Sleep(5000);
            }
        }

        #endregion

    }
}
