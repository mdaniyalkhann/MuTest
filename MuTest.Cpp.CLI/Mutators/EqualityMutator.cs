using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class EqualityMutator : Mutator, IMutator
    {
        public EqualityMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                [" == "] = new List<string> { " != " },
                [" != "] = new List<string> { " == " },
                [" < "] = new List<string> { " > " },
                [" > "] = new List<string> { " < " },
                [" <= "] = new List<string> { " > " },
                [" >= "] = new List<string> { " < " }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Equality;

        public string Description { get; } = "EQUALITY [==, !=, <, >, <=, >=]";

        public bool DefaultMutant { get; } = true;
    }
}