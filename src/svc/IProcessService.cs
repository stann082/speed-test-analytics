namespace service
{
    public interface IProcessService
    {

        string StandardOutput { get; }
        bool Run(string programPath, string arguments);

    }
}
