using Microsoft.Extensions.Configuration;
using Serilog;
using service;
using System;
using System.IO;
using System.Text;

namespace speed_test
{
    public class Application
    {

        #region Constants

        private const string SPEED_TEST_FILE_PATH = "SpeedTestFilePath";
        private const string TABLE_NAME = "SpeedTestAnalytics";

        #endregion

        #region Constructors

        public Application(IConfiguration config, IAwsDynamoDbService dynamoDbService, IProcessService processService)
        {
            Config = config;
            DynamoDbService = dynamoDbService;
            ProcessService = processService;
        }

        #endregion

        #region Properties

        private IConfiguration Config { get; set; }
        private IAwsDynamoDbService DynamoDbService { get; set; }
        private IProcessService ProcessService { get; set; }

        #endregion

        #region Application Starting Point

        public void Run()
        {
            string speedTestFile = GetSpeedTestFilePath();
            if (string.IsNullOrEmpty(speedTestFile))
            {
                return;
            }

            DynamoDbService.CreateTable(TABLE_NAME);

            string machineId = $"{Environment.MachineName} ({Environment.OSVersion.Platform})";
            Log.Information($"Running a speed test for {machineId}...");

            if (!ProcessService.Run(speedTestFile, BuildArguments()))
            {
                Log.Information("Speed test run failed... Please check logs for details.");
                return;
            }

            Log.Information($"Adding speed test result entry to the {TABLE_NAME} table...");
            DynamoDbService.PutItem(TABLE_NAME, machineId, ProcessService.StandardOutput);
            Log.Information("An entry had been successfully added!");
        }

        #endregion

        #region Helper Methods

        private string BuildArguments()
        {
            StringBuilder sb = new StringBuilder();

            string formatType = Config.GetSection("OutputFormat").Value;
            sb.Append(IsValidFormatType(formatType) ? $"--format={formatType}" : "--format=json");
            return sb.ToString();
        }

        private string GetSpeedTestFilePath()
        {
            string filePath = EnvironmentVariableProvider.GetEnvironmentVariable(SPEED_TEST_FILE_PATH);
            if (string.IsNullOrEmpty(filePath))
            {
                Log.Information($"Environment variable {SPEED_TEST_FILE_PATH} not set.");
                return string.Empty;
            }

            if (!File.Exists(filePath))
            {
                Log.Information($"File {filePath} does not exist.");
                return string.Empty;
            }

            return filePath;
        }

        private bool IsValidFormatType(string formatType)
        {
            if (!string.IsNullOrEmpty(formatType))
            {
                return false;
            }

            return formatType is "json" or "jsonl" or "json-pretty";
        }

        #endregion

    }
}
