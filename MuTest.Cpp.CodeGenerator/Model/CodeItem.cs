#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class CodeItem
    {
        public long Id { get; set; }
        public long FileId { get; set; }
        public long ParentId { get; set; }
        public long Kind { get; set; }
        public long Attributes { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long StartColumn { get; set; }
        public long StartLine { get; set; }
        public long EndColumn { get; set; }
        public long EndLine { get; set; }
        public long NameStartColumn { get; set; }
        public long NameStartLine { get; set; }
        public long NameEndColumn { get; set; }
        public long NameEndLine { get; set; }
        public string ParamDefaultValue { get; set; }
        public long? ParamDefaultValueStartColumn { get; set; }
        public long? ParamDefaultValueStartLine { get; set; }
        public long? ParamDefaultValueEndColumn { get; set; }
        public long? ParamDefaultValueEndLine { get; set; }
        public long? ParamNumber { get; set; }
        public string LowerNameHint { get; set; }
    }
}
