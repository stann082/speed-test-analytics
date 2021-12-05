using System.Diagnostics;

namespace service
{
    public class ProcessRunnerService
    {

        #region Constructors

        public ProcessRunnerService(string programPath, string arguments)
        {
            ProgramPath = programPath;
            Arguments = arguments;
        }

        #endregion

        #region Properties

        public string StandardOutput { get; private set; }

        private string Arguments { get; }
        private string ProgramPath { get; }

        #endregion

        #region Public Methods

        public void Run()
        {
            var psi = new ProcessStartInfo();
            psi.FileName = ProgramPath;
            psi.Arguments = Arguments;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;

            using var process = Process.Start(psi);
            using StreamReader reader = process.StandardOutput;

            StandardOutput = reader.ReadToEnd();
        }

        #endregion

    }
}
