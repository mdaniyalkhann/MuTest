using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class PrePostfixUnaryMutator : Mutator, IMutator
    {
        public PrePostfixUnaryMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                ["\\+\\+"] = new List<string> { "--" },
                ["--"] = new List<string> { "++" }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Unary;

        public string Description { get; } = "UNARY [++, --]";

        public bool DefaultMutant { get; } = false;
    }
}