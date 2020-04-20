using MuTest.Core.Mutants;
using Newtonsoft.Json;

namespace MuTest.Cpp.CLI.Mutants
{
    public class CppMutant
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mutation")]
        public CppMutation Mutation { get; set; }

        [JsonIgnore]
        public MutantStatus ResultStatus { get; set; } = MutantStatus.NotRun;

        [JsonProperty("status")]
        public string Status => ResultStatus.ToString();
    }
}
