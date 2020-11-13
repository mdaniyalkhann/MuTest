#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class ProjectRef
    {
        public long ConfigId { get; set; }
        public long ResolvedName { get; set; }
        public long ProjectRefName { get; set; }
        public long ProjectRefGuid { get; set; }

        public virtual Config Config { get; set; }
    }
}
