using System.Collections.Generic;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public interface IAridNodeMutantFilterer
    {
        IList<CppMutant> FilterMutants(IList<CppMutant> mutants);
    }
}