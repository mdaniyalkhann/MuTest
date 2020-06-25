using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class LogicalConnectorMutator : Mutator, IMutator
    {
        public LogicalConnectorMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                [" && "] = new List<string> { " || " },
                [" \\|\\| "] = new List<string> { " && " },
                ["!"] = new List<string> { string.Empty }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Logical;

        public string Description { get; } = "LOGICAL [&&, ||, !]";

        public bool DefaultMutant { get; } = true;
    }
}