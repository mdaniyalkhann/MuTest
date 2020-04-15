using System;
using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class IssueJson
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("self")]
        public Uri Self { get; set; }
    }
}