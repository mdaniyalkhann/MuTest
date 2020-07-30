using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MuTest.Core.Common
{
    public interface IBuildExecutor
    {
        Constants.BuildExecutionStatus LastBuildStatus { get; }

        string OutputPath { get; set; }

        string BaseAddress { get; set; }

        string IntermediateOutputPath { get; set; }

        event EventHandler<string> OutputDataReceived;

        event EventHandler<string> BeforeMsBuildExecuted;

        Task ExecuteBuildInDebugModeWithDependencies();

        Task ExecuteBuildInDebugModeWithoutDependencies();

        Task ExecuteBuildInReleaseModeWithDependencies();

        Task ExecuteBuildInReleaseModeWithoutDependencies();
    }
}