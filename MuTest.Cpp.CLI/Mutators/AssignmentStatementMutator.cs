using System.Collections.Generic;
using MuTest.Core.Mutators;

namespace MuTest.Cpp.CLI.Mutators
{
    internal class AssignmentStatementMutator : Mutator, IMutator
    {
        public AssignmentStatementMutator()
        {
            KindsToMutate = new Dictionary<string, IList<string>>
            {
                ["\\+="] = new List<string> { " = " },
                ["-="] = new List<string> { " += " },
                ["\\*="] = new List<string> { " /= " },
                ["\\/="] = new List<string> { " *= " },
                ["%="] = new List<string> { " *= " },
                ["&="] = new List<string> { " |= " },
                ["\\|="] = new List<string> { " &= " },
                ["\\^="] = new List<string> { " &= " },
                ["<<="] = new List<string> { " >>= " },
                [">>="] = new List<string> { " <<= " }
            };
        }

        public override MutatorType MutatorType { get; } = MutatorType.Assignment;

        public string Description { get; } = "ASSIGNMENT [+=, -=, x=, %=, |=, &=, <<=, >>=, ^=]";

        public bool DefaultMutant { get; } = false;
    }
}