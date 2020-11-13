#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class CodeItemKind
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ParserGuid { get; set; }

        public virtual Parser ParserGu { get; set; }
    }
}
