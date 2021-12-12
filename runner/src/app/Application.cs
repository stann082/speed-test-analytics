using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private const string SPEED_TEST_FILE_NAME = "speedtest.exe";
        private const string SPEED_TEST_FILE_PATH_ENV_VAR = "SpeedTestFilePath";
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

            SpeedTestResult result = CreateSpeedTestResult(ProcessService.StandardOutput);
            Log.Information("The test result is: {@SpeedTestResult}", result);

            Log.Information($"Adding speed test result entry to the {TABLE_NAME} table...");
            DynamoDbService.PutItem(TABLE_NAME, machineId, ProcessService.StandardOutput);
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

        private SpeedTestResult CreateSpeedTestResult(string output)
        {
            var parsedObject = JObject.Parse(output);

            string downloadDataJson = parsedObject["download"].ToString();
            SpeedTestResponseData downloadData = JsonConvert.DeserializeObject<SpeedTestResponseData>(downloadDataJson);

            string uploadDataJson = parsedObject["upload"].ToString();
            SpeedTestResponseData uploadData = JsonConvert.DeserializeObject<SpeedTestResponseData>(uploadDataJson);

            return new SpeedTestResult(downloadData, uploadData);
        }

        private string GetSpeedTestFilePath()
        {
            // file already present in a deploy directory
            if (File.Exists(SPEED_TEST_FILE_NAME))
            {
                return SPEED_TEST_FILE_NAME;
            }

            // get file path from an environment variable
            string speedTestFilePathEnvVar = EnvironmentVariableProvider.GetEnvironmentVariable(SPEED_TEST_FILE_PATH_ENV_VAR);
            if (string.IsNullOrEmpty(speedTestFilePathEnvVar))
            {
                Log.Information($"Environment variable {SPEED_TEST_FILE_PATH_ENV_VAR} not set.");
                return string.Empty;
            }

            string speedTestFilePath = Environment.ExpandEnvironmentVariables(speedTestFilePathEnvVar);
            if (!File.Exists(speedTestFilePath))
            {
                Log.Information($"File {speedTestFilePath} does not exist.");
                return string.Empty;
            }

            return speedTestFilePath;
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
