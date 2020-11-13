#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class AssocText
    {
        public long CodeItemId { get; set; }
        public long Kind { get; set; }
        public string Text { get; set; }

        public virtual CodeItem CodeItem { get; set; }
    }
}
