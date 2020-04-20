using System.IO;

namespace MuTest.Cpp.CLI.Model
{
    internal class CppTestContext
    {
        public int Index { get; set; }

        public FileInfo TestClass { get; set; }

        public FileInfo SourceClass { get; set; }
    }
}
