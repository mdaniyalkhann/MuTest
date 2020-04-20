using System.Collections.Generic;

namespace MuTest.Cpp.CLI.Mutants
{
    public interface IMutantOrchestrator
    {
        IEnumerable<CppMutant> GetLatestMutantBatch();
        void Mutate(string sourceFile);
    }
}