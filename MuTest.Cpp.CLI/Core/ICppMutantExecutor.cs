using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;

namespace MuTest.Cpp.CLI.Core
{
    public interface ICppMutantExecutor
    {
        double SurvivedThreshold { get; set; }

        double KilledThreshold { get; set; }

        bool CancelMutationOperation { get; set; }

        bool EnableDiagnostics { get; set; }

        string LastExecutionOutput { get; set; }

        int NumberOfMutantsExecutingInParallel { get; set; }

        event EventHandler<CppMutantEventArgs> MutantExecuted;

        void OnMutantExecuted(CppMutantEventArgs args);

        Task ExecuteMutants();

        void PrintMutatorSummary(StringBuilder mutationProcessLog, IList<CppMutant> mutants);

        void PrintClassSummary(CppClass cppClass, StringBuilder mutationProcessLog);
    }
}