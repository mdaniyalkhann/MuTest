#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class ConfigFile
    {
        public long ConfigId { get; set; }
        public long FileId { get; set; }
        public long Implicit { get; set; }
        public long Reference { get; set; }
        public long Compiled { get; set; }
        public long CompiledPch { get; set; }
        public long Explicit { get; set; }
        public long Shared { get; set; }
        public long Generated { get; set; }
        public long ConfigFinal { get; set; }
        public long IncludePath { get; set; }
        public long FrameworkIncludePath { get; set; }
        public long Options { get; set; }

        public virtual Config Config { get; set; }
        public virtual File File { get; set; }
    }
}
