using System.Collections.Generic;
using MuTest.Core.Mutants;

namespace MuTest.Core.Common
{
    public interface IMutantSelector
    {
        IList<Mutant> SelectMutants(int numberOfMutantsPerLine, IList<Mutant> mutants);
    }
}