using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class Status
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}