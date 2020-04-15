using System.Collections.Generic;
using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class IssuesJson
    {
        [JsonProperty("issues")]
        public List<IssueJson> Issues { get; set; } = new List<IssueJson>();

        public static IssuesJson FromJson(string json) => JsonConvert.DeserializeObject<IssuesJson>(json, Converter.Settings);
    }
}