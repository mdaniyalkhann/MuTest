using System.IO;

namespace MuTest.Cpp.CLI.Model
{
    public class CppTestContext
    {
        public int Index { get; set; }

        public FileInfo TestClass { get; set; }

        public FileInfo SourceClass { get; set; }

        public FileInfo BackupSourceClass { get; set; }

        public FileInfo SourceHeader { get; set; }
    }
}
