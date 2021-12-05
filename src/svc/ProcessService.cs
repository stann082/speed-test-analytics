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
                    // TODO: log error
                    return false;
                }

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    // TODO: log error
                    return false;
                }

                using StreamReader reader = process.StandardOutput;
                StandardOutput = reader.ReadToEnd();
                return !string.IsNullOrEmpty(StandardOutput);
            }
            catch (Exception)
            {
                // TODO: log exception
                return false;
            }
        }

        #endregion

    }
}
