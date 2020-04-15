using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class TargetClass
    {
        [JsonProperty("class-name")]
        public string ClassName { get; set; }

        [JsonProperty("class-path")]
        public string ClassPath { get; set; }

        [JsonProperty("test-class-path")]
        public string TestClassPath { get; set; }
    }
}
