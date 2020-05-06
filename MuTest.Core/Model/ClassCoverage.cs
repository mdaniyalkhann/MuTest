using System.Collections.Generic;
using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class ClassCoverage
    {
        [JsonProperty("class-name")]
        public string ClassName { get; set; }

        [JsonProperty("class-path")]
        public string ClassPath { get; set; }

        [JsonProperty("code-coverage")]
        public Coverage Coverage { get; set; }

        [JsonProperty("mutants-count")]
        public int NumberOfMutants { get; set; }

        [JsonProperty("mutants-lines")]
        public List<int> MutantsLines { get; } = new List<int>();

        [JsonIgnore]
        public bool Excluded { get; set; }

        [JsonProperty("zero-survived-mutants")]
        public bool ZeroSurvivedMutants { get; set; }

        public override string ToString()
        {
            var excluded = Excluded
                ? " [Auto Generated, Model, DTO or Entity]"
                : string.Empty;

            var zeroMutantsSurvived = ZeroSurvivedMutants
                ? " [All Mutants are Killed]"
                : string.Empty;
            return $"External coverage in Class [{ClassName}: {Coverage.LinesCovered}/{Coverage.TotalLines}] [Mutants: {NumberOfMutants}]{excluded}{zeroMutantsSurvived}";
        }
    }
}
