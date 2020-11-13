#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class Config
    {
        public long Id { get; set; }
        public long Hash { get; set; }
        public long ProjectId { get; set; }
        public string Name { get; set; }
        public string ToolsetIsenseIdentifier { get; set; }
        public long ExcludePath { get; set; }
        public long ConfigIncludePath { get; set; }
        public long ConfigFrameworkIncludePath { get; set; }
        public long ConfigOptions { get; set; }
        public long PlatformIncludePath { get; set; }
        public long PlatformFrameworkIncludePath { get; set; }
        public long PlatformOptions { get; set; }

        public virtual Project Project { get; set; }
    }
}
