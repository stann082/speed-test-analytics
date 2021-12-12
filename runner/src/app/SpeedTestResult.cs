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
            DownloadSpeed = ConvertToHumanReadableFormat(downloadData.Bandwidth, true);
            DownloadTime = $"{downloadData.Elapsed / 1000} secs";

            UploadBytes = ConvertToHumanReadableFormat(uploadData.Bytes);
            UploadSpeed = ConvertToHumanReadableFormat(uploadData.Bandwidth, true);
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

        private string ConvertToHumanReadableFormat(uint size, bool useBits = false)
        {
            string[] sizes;
            if (useBits)
            {
                sizes = new string[] { "Bit", "KBit", "MBit", "GBit" };
            }
            else
            {
                sizes = new string[] { "B", "KB", "MB", "GB" };
            }

            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            if (useBits)
            {
                size *= 8;
            }

            return string.Format("{0:0.##} {1}", size, sizes[order]);
        }

        #endregion

    }
}
