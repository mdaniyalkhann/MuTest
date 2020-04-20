using System.Collections.Generic;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Mutators
{
    public interface IMutator
    {
        IEnumerable<CppMutant> Mutate(CodeLine node);

        string Description { get; }

        bool DefaultMutant { get; }
    }
}
