using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class BooleanMutator : Mutator, IMutator
    {
        public BooleanMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                ["true"] = new List<string> { "false" },
                ["false"] = new List<string> { "true" }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Boolean;

        public string Description { get; } = "BOOLEAN [true, false]";

        public bool DefaultMutant { get; } = false;
    }
}