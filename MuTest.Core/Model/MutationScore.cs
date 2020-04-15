using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class MutationScore
    {
        [JsonProperty("survived")]
        public int Survived { get; set; }

        [JsonProperty("killed")]
        public int Killed { get; set; }

        [JsonProperty("uncovered")]
        public int Uncovered { get; set; }

        [JsonProperty("timeout")]
        public int Timeout { get; set; }

        [JsonProperty("build-errors")]
        public int BuildErrors { get; set; }

        [JsonProperty("skipped")]
        public int Skipped { get; set; }

        [JsonProperty("covered")]
        public int Covered { get; set; }

        [JsonProperty("coverage")]
        public decimal Coverage { get; set; }

        [JsonProperty("mutation")]
        public string Mutation =>
            Covered == 0
                ? "N/A"
                : $"{Killed}/{Covered}[{Coverage:P}]";

        public override string ToString()
        {
            return $"Mutation Status: Survived({Survived}) Killed({Killed}) Not Covered({Uncovered}) Timeout({Timeout}) Build Errors({BuildErrors}) Skipped({Skipped})";
        }
    }
}
