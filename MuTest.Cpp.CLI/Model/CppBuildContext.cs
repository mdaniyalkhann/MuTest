using System.Collections.Generic;
using System.IO;

namespace MuTest.Cpp.CLI.Model
{
    public class CppBuildContext
    {
        public FileInfo TestProject { get; set; }

        public FileInfo TestSolution { get; set; }

        public string OutputPath { get; set; }

        public string IntermediateOutputPath { get; set; }

        public string OutDir { get; set; }

        public string IntDir { get; set; }

        public IList<CppTestContext> TestContexts { get; } = new List<CppTestContext>();
    }
}
