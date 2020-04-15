using MuTest.Core.Model;
using Newtonsoft.Json;

namespace MuTest.Console.Options
{
    public class JsonOptions
    {
        [JsonProperty("options")]
        public MuTestOptions Options { get; set; }

        [JsonProperty("result")]
        public SourceClassDetail Result { get; set; }
    }
}
