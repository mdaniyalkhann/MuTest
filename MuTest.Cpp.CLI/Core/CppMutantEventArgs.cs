using System;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public class CppMutantEventArgs: EventArgs
    {
        public CppMutant Mutant { get; set; }

        public string TestLog { get; set; }

        public string BuildLog { get; set; }
    }
}
