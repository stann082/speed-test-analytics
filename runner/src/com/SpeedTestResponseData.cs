using Newtonsoft.Json;

namespace domain
{
    public class SpeedTestResponseData
    {

        [JsonProperty("bandwidth")]
        public uint Bandwidth { get; set; }

        [JsonProperty("bytes")]
        public uint Bytes { get; set; }

        [JsonProperty("elapsed")]
        public int Elapsed { get; set; }

    }
}
