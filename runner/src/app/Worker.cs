using domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using service;
using System.Text;

namespace worker;

public class Worker : BackgroundService
{

    #region Constants

    private const int DEFAULT_RUN_TIME_FREQUENCY = 60;
    private const string SPEED_TEST_FILE_NAME = "speedtest.exe";
    private const string SPEED_TEST_FILE_PATH_ENV_VAR = "SpeedTestFilePath";
    private const string TABLE_NAME = "SpeedTestAnalytics";

    #endregion

    #region Fields

    private readonly IConfiguration _config;
    private readonly IAwsDynamoDbService _dynamoDbService;
    private readonly IProcessService _processService;

    #endregion

    #region Constructors

    public Worker(IConfiguration config, IAwsDynamoDbService dynamoDbService, IProcessService processService)
    {
        _config = config;
        _dynamoDbService = dynamoDbService;
        _processService = processService;
    }

    #endregion

    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            RunSpeedTest();
            await Task.Delay(1000 * 60 * GetRunFrequencyMinutes(), stoppingToken);
        }
    }

    #endregion

    #region Helper Methods

    private string BuildArguments()
    {
        StringBuilder sb = new StringBuilder();

        string formatType = _config.GetSection("OutputFormat").Value;
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

    private int GetRunFrequencyMinutes()
    {
        string minutesValue = _config.GetSection("RunFrequencyMinutes").Value;
        return int.TryParse(minutesValue, out int minutes) ? minutes : DEFAULT_RUN_TIME_FREQUENCY;
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

    private void RunSpeedTest()
    {
        string speedTestFile = GetSpeedTestFilePath();
        if (string.IsNullOrEmpty(speedTestFile))
        {
            return;
        }

        _dynamoDbService.CreateTable(TABLE_NAME);

        string machineId = $"{Environment.MachineName} ({Environment.OSVersion.Platform})";
        Log.Information($"Running a speed test for {machineId}...");

        if (!_processService.Run(speedTestFile, BuildArguments()))
        {
            Log.Information("Speed test run failed... Please check logs for details.");
            return;
        }

        SpeedTestResult result = CreateSpeedTestResult(_processService.StandardOutput);
        Log.Information("The test result is: {@SpeedTestResult}", result);

        Log.Information($"Adding speed test result entry to the {TABLE_NAME} table...");
        _dynamoDbService.PutItem(TABLE_NAME, machineId, _processService.StandardOutput);
    }

    #endregion

}
