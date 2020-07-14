using System.Collections.Generic;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public interface IMutantSelector
    {
        IList<CppMutant> SelectMutants(int numberOfMutantsPerLine, IList<CppMutant> mutants);
    }
}