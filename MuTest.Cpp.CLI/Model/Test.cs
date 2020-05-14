using Newtonsoft.Json;
namespace MuTest.Cpp.CLI.Model
{
    public class Test
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("execution-time")]
        public double ExecutionTime { get; set; }
    }
}
