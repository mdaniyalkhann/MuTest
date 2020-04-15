using System;
using MuTest.Core.Mutants;

namespace MuTest.Core.Common
{
    public class MutantEventArgs: EventArgs
    {
        public Mutant Mutant { get; set; }

        public string TestLog { get; set; }

        public string BuildLog { get; set; }
    }
}
