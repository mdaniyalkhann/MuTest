using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Model;

namespace MuTest.Core.Common
{
    public interface ITestExecutor
    {
        Constants.TestExecutionStatus LastTestExecutionStatus { get; }

        string FullyQualifiedName { get; set; }

        string BaseAddress { get; set; }

        bool EnableTimeout { get; set; }

        TestRun TestResult { get; }

        CoverageDS CodeCoverage { get; }

        bool EnableCustomOptions { get; set; }

        bool EnableLogging { get; set; }

        bool X64TargetPlatform { get; set; }

        event EventHandler<string> OutputDataReceived;

        Task ExecuteTests(IList<MethodDetail> selectedMethods);

        string PrintTestResult(TestRun testReport);
    }
}