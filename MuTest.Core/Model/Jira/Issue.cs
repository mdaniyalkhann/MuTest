using System;
using Newtonsoft.Json;

namespace MuTest.Core.Model.Jira
{
    public class Issue
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("fields")]
        public Fields Fields { get; set; }
    }
}