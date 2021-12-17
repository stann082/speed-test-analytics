namespace domain
{
    public class NullSpeedTestResult : ISpeedTestResult
    {

        public static readonly ISpeedTestResult Singleton = new NullSpeedTestResult();

        #region Constructors

        private NullSpeedTestResult()
        {
        }

        #endregion

        #region ISpeedTestResult Members

        public string DownloadBytes => string.Empty;
        public string DownloadSpeed => string.Empty;
        public string DownloadTime => string.Empty;

        public string UploadBytes => string.Empty;
        public string UploadSpeed => string.Empty;
        public string UploadTime => string.Empty;

        #endregion

    }
}
