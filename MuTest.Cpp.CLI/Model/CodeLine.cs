using System.Collections.Generic;

namespace MuTest.Cpp.CLI.Model
{
    public class CodeLine
    {
        public string Line { get; set; }

        public int LineNumber { get; set; }

        public List<StringLine> StringLines { get; } = new List<StringLine>();
    }
}
