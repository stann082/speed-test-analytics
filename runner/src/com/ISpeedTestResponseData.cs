namespace domain
{
    public interface ISpeedTestResponseData
    {

        uint Bandwidth { get; }
        uint Bytes { get; }
        int Elapsed { get; }

    }
}
