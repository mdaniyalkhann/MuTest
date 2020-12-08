using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class RelationalOperatorMutator : Mutator, IMutator
    {
        public RelationalOperatorMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                ["=="] = new List<string> { "!=" },
                ["!="] = new List<string> { "==" },
                ["<"] = new List<string> { ">" },
                [">"] = new List<string> { "<" },
                ["<="] = new List<string> { ">" },
                [">="] = new List<string> { "<" }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Relational;

        public string Description { get; } = "RELATIONAL [==, !=, <, >, <=, >=]";

        public bool DefaultMutant { get; } = true;
    }
}