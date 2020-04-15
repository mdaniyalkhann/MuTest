using System.Collections.Generic;
using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class SearchJson
    {
        [JsonProperty("issues")]
        public List<Issue> Issues { get; set; } = new List<Issue>();

        public static SearchJson FromJson(string json) => JsonConvert.DeserializeObject<SearchJson>(json, Converter.Settings);
    }
}
