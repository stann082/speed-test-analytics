using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace speed_test
{
    internal class SpeedTestResult
    {

        #region Constructors

        public SpeedTestResult(SpeedTestResponseData downloadData, SpeedTestResponseData uploadData)
        {
            DownloadBytes = ConvertToHumanReadableFormat(downloadData.Bytes);
            DownloadSpeed = ConvertToHumanReadableFormat(downloadData.Bandwidth);
            DownloadTime = $"{downloadData.Elapsed / 1000} secs";

            UploadBytes = ConvertToHumanReadableFormat(uploadData.Bytes);
            UploadSpeed = ConvertToHumanReadableFormat(uploadData.Bandwidth);
            UploadTime = $"{uploadData.Elapsed / 1000} secs";
        }

        #endregion

        #region Properties

        public string DownloadBytes { get; }
        public string DownloadSpeed { get; }
        public string DownloadTime { get; }

        public string UploadBytes { get; }
        public string UploadSpeed { get; }
        public string UploadTime { get; }

        #endregion

        #region Helper Methods

        private string ConvertToHumanReadableFormat(uint byteSize)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (byteSize >= 1024 && order < sizes.Length - 1)
            {
                order++;
                byteSize /= 1024;
            }

            return string.Format("{0:0.##} {1}", byteSize, sizes[order]);
        }

        #endregion

    }
}
