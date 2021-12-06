using Newtonsoft.Json;

namespace speed_test
{
    internal class SpeedTestResponseData
    {

        [JsonProperty("bandwidth")]
        public uint Bandwidth { get; set; }

        [JsonProperty("bytes")]
        public uint Bytes { get; set; }

        [JsonProperty("elapsed")]
        public int Elapsed { get; set; }

    }
}
