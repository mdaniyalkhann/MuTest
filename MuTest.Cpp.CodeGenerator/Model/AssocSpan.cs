#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class AssocSpan
    {
        public long CodeItemId { get; set; }
        public long Kind { get; set; }
        public long StartColumn { get; set; }
        public long StartLine { get; set; }
        public long EndColumn { get; set; }
        public long EndLine { get; set; }

        public virtual CodeItem CodeItem { get; set; }
    }
}
