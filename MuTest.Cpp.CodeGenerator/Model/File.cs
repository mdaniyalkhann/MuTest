#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class File
    {
        public long Id { get; set; }
        public long Timestamp { get; set; }
        public long Parsetime { get; set; }
        public long Addtime { get; set; }
        public long Difftime { get; set; }
        public string Name { get; set; }
        public string LeafName { get; set; }
        public long Attributes { get; set; }
        public string ParserGuid { get; set; }

        public virtual Parser ParserGu { get; set; }
    }
}
