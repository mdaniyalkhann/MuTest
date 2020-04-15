using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class Fields
    {
        [JsonProperty("summary")]
        public string Summary { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }
    }
}