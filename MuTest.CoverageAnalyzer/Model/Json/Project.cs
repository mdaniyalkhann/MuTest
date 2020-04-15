using Newtonsoft.Json;

namespace MuTest.CoverageAnalyzer.Model.Json
{
    public partial class Project
    {
        [JsonProperty("RepositoryName")]
        public string RepositoryName { get; set; }

        [JsonProperty("Solutions")]
        public string[] Solutions { get; set; }

        [JsonProperty("ProjectID")]
        public string ProjectId { get; set; }

        [JsonProperty("Branch")]
        public string Branch { get; set; }

        [JsonProperty("FakesContainerProject")]
        public string FakesContainerProject { get; set; }

        [JsonProperty("FakesContainerGenerated")]
        public string FakesContainerGenerated { get; set; }

        [JsonProperty("SourcePath")]
        public string SourcePath { get; set; }

        [JsonProperty("SourceLibrary")]
        public string SourceLibrary { get; set; }

        [JsonProperty("TestPath")]
        public string TestPath { get; set; }

        [JsonProperty("TestLibrary")]
        public string TestLibrary { get; set; }

        [JsonProperty("CommonFakesProject")]
        public string CommonFakesProject { get; set; }

        [JsonProperty("MergeMode")]
        public int MergeMode { get; set; }

        [JsonProperty("Classes")]
        public Class[] Classes { get; set; }
    }

    public partial class Project
    {
        public static Project FromJson(string json) => JsonConvert.DeserializeObject<Project>(json, Converter.Settings);
    }
}
