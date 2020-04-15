using System.Collections.Generic;
using System.Threading.Tasks;
using MuTest.Core.Model;

namespace MuTest.Core.Common
{
    public interface IMutantAnalyzer
    {
        IMutantExecutor MutantExecutor { get; }

        int TotalMutants { get; }

        int MutantProgress { get; }

        double KilledThreshold { get; set; }

        double SurvivedThreshold { get; set; }

        bool EnableDiagnostics { get; set; }

        int ConcurrentTestRunners { get; set; }

        List<int> ExternalCoveredMutants { get; }

        string Specific { get; set; }

        string RegEx { get; set; }

        bool ExecuteAllTests { get; set; }

        bool IncludeNestedClasses { get; set; }

        bool UseExternalCodeCoverage { get; set; }

        string ProcessWholeProject { get; set; }

        string TestClass { get; set; }

        bool IncludePartialClasses { get; set; }

        string TestProject { get; set; }

        string TestProjectLibrary { get; set; }

        bool X64TargetPlatform { get; set; }

        bool BuildInReleaseMode { get; set; }

        string SourceProjectLibrary { get; set; }

        bool NoCoverage { get; set; }

        bool UseClassFilter { get; set; }

        char ProgressIndicator { get; set; }

        Task<SourceClassDetail> Analyze(string sourceClass, string className, string sourceProject);
    }
}