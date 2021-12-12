using Serilog;
using System.Diagnostics;

namespace service
{
    public class ProcessService : IProcessService
    {

        #region Constructors

        public ProcessService()
        {
            StandardOutput = string.Empty;
        }

        #endregion

        #region Properties

        public string StandardOutput { get; private set; }

        #endregion

        #region Public Methods

        public bool Run(string programPath, string arguments)
        {
            var psi = new ProcessStartInfo();
            psi.FileName = programPath;
            psi.Arguments = arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            try
            {
                using var process = Process.Start(psi);
                if (process == null)
                {
                    LogError("Something went wrong... Process did not start.");
                    return false;
                }

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    LogError($"Something went wrong... Process exited with code {process.ExitCode}.");
                    return false;
                }

                using StreamReader reader = process.StandardOutput;
                StandardOutput = reader.ReadToEnd();
                return !string.IsNullOrEmpty(StandardOutput);
            }
            catch (Exception ex)
            {
                LogError($"An error has occurred during speed test run.", ex);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private static void LogError(string message, Exception? exception = null)
        {
            if (exception != null)
            {
                Log.Error(exception, message);
            }
            else
            {
                Log.Error(message);
            }
        }

        #endregion

    }
}
