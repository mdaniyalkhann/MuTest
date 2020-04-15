using Newtonsoft.Json;

namespace MuTest.CoverageAnalyzer.Options
{
    public class JsonOptions
    {
        [JsonProperty("options")]
        public CoverageAnalyzerOptions Options { get; set; }
    }
}
