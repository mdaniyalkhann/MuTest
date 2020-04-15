using MuTest.Core.Model;
using Newtonsoft.Json;

namespace MuTest.Console.Model
{
    public class ClassSummary
    {
        [JsonProperty("target-class")]
        public TargetClass TargetClass { get; set; }

        [JsonProperty("mutation-score")]
        public MutationScore MutationScore { get; set; }

        [JsonProperty("code-coverage")]
        public Coverage Coverage { get; set; }
    }
}
