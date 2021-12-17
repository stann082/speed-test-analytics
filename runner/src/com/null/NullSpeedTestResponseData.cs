namespace domain
{
    public class NullSpeedTestResponseData : ISpeedTestResponseData
    {

        public static readonly ISpeedTestResponseData Singleton = new NullSpeedTestResponseData();
        
        #region Constructors

        private NullSpeedTestResponseData()
        {
        }

        #endregion

        #region ISpeedTestResponseData Members

        public uint Bandwidth => 0;
        public uint Bytes => 0;
        public int Elapsed => 0;

        #endregion

    }
}
