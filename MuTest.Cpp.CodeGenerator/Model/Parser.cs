using System.Collections.Generic;

#nullable disable

namespace MuTest.Cpp.CodeGenerator.Model
{
    public class Parser
    {
        private ICollection<CodeItemKind> _codeItemKinds;
        private ICollection<File> _files;

        public string ParserGuid { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }

        public virtual ICollection<CodeItemKind> CodeItemKinds
        {
            get => _codeItemKinds ?? new HashSet<CodeItemKind>();
            set => _codeItemKinds = value;
        }

        public virtual ICollection<File> Files
        {
            get => _files ?? new HashSet<File>();
            set => _files = value;
        }
    }
}
