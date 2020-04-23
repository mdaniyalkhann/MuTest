using MuTest.Cpp.CLI.Model;
using Newtonsoft.Json;

namespace MuTest.Cpp.CLI.Options
{
    public class JsonOptions
    {
        [JsonProperty("options")]
        public MuTestOptions Options { get; set; }

        [JsonProperty("result")]
        public CppClass Result { get; set; }
    }
}
