using System.Collections.Generic;
using System.IO;
using MuTest.Core.Model.ClassDeclarations;
using Newtonsoft.Json;

namespace MuTest.Core.Model
{
    public class ClassDetail
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonProperty("class-name")]
        public string FullName { get; set; }

        [JsonIgnore]
        public string DisplayName => $"{FullName} [{Path.GetFileName(FilePath)}]";

        [JsonIgnore]
        public string FilePath { get; set; }

        [JsonIgnore]
        public string ClassProject { get; set; }

        [JsonIgnore]
        public string ClassLibrary { get; set; }

        [JsonIgnore]
        public int TotalNumberOfMethods { get; set; }

        [JsonIgnore]
        public ClassDeclaration Claz { get; set; }

        [JsonIgnore]
        public bool BuildInReleaseMode { get; set; }

        [JsonIgnore]
        public bool X64TargetPlatform { get; set; }

        [JsonIgnore]
        public bool DoNetCoreProject { get; set; }

        [JsonProperty("methods")]
        public List<MethodDetail> MethodDetails { get; } = new List<MethodDetail>();
    }
}
