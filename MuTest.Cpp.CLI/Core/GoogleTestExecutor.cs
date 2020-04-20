using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Model;

namespace MuTest.Cpp.CLI.Core
{
    public class GoogleTestExecutor
    {
        private const string TestCaseFilter = " --gtest_filter=";
        private const string FailedDuringExecution = "[  FAILED  ]";

        public bool KillProcessOnTestFail { get; set; } = false;

        public Constants.TestExecutionStatus LastTestExecutionStatus { get; private set; }

        public TestRun TestResult { get; private set; }

        public event EventHandler<string> OutputDataReceived;

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            OutputDataReceived?.Invoke(this, arg.Data);
        }

        public async Task ExecuteTests(string app, string filter)
        {
            LastTestExecutionStatus = Constants.TestExecutionStatus.Success;
            TestResult = null;
            var methodBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                methodBuilder.Append($" {TestCaseFilter}")
                    .Append($"\"{filter}\"");
            }

            var processInfo = new ProcessStartInfo(app)
            {
                Arguments = methodBuilder.ToString(),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            await Task.Run(() =>
            {
                using (var process = new Process
                {
                    StartInfo = processInfo,
                    EnableRaisingEvents = true
                })
                {
                    process.OutputDataReceived += ProcessOnOutputDataReceived;
                    process.ErrorDataReceived += ProcessOnOutputDataReceived;
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (LastTestExecutionStatus != Constants.TestExecutionStatus.Failed)
                    {
                        LastTestExecutionStatus = Constants.TestStatusList[process.ExitCode];
                    }

                    process.OutputDataReceived -= ProcessOnOutputDataReceived;
                    process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                }
            });
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnThresholdReached(args);

            if (KillProcessOnTestFail && args.Data != null &&
                args.Data.StartsWith(FailedDuringExecution))
            {
                LastTestExecutionStatus = Constants.TestExecutionStatus.Failed;

                var process = (Process)sender;
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
        }
    }
}