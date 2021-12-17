namespace domain
{
    public interface ISpeedTestResult
    {

        string DownloadBytes { get; }
        string DownloadSpeed { get; }
        string DownloadTime { get; }

        string UploadBytes { get; }
        string UploadSpeed { get; }
        string UploadTime { get; }

    }
}
