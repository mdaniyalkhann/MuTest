using Newtonsoft.Json;

namespace MuTest.CoverageAnalyzer.Model.Json
{
    public class Hut
    {
        [JsonProperty("IssueKey")]
        public string IssueKey { get; set; }

        [JsonProperty("CoveredCount")]
        public uint LinesCoveredCount { get; set; }

        [JsonProperty("CoveredRatio")]
        public decimal LinesCoveredRatio { get; set; }

        [JsonProperty("TotalCount")]
        public uint TotalLines { get; set; }
    }
}
