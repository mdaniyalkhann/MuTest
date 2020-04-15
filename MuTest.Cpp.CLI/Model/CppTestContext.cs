using System.IO;

namespace MuTest.Cpp.CLI.Model
{
    internal class CppTestContext
    {
        public int Index { get; set; }

        public FileInfo TestClass { get; set; } 

        public FileInfo TestProject { get; set; }

        public FileInfo TestSolution { get; set; }

        public FileInfo SourceClass { get; set; }

        public FileInfo OutputPath { get; set; }

        public FileInfo IntermediateOutputPath { get; set; }

        public FileInfo OutDir { get; set; }

        public FileInfo IntDir { get; set; }
    }
}
