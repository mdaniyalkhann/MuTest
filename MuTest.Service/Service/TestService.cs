using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model.Service;

namespace MuTest.Service.Service
{
    internal class TestService
    {
        private readonly MuTestSettings _settings;
        private StringBuilder _testOutput;
        private TestResult _testResult;
        private const string FailedDuringExecution = "Failed ";
        private const string CoverageExtension = ".coverage";
        private const string ErrorDuringExecution = "  X ";

        public bool KillProcessOnTestFail { get; set; }

        public TestService(MuTestSettings settings)
        {
            _settings = settings;
        }

        public async Task<TestResult> Test(string options)
        {
            _testResult = new TestResult();
            try
            {
                var processInfo = new ProcessStartInfo(_settings.VSTestConsolePath)
                {
                    Arguments = options,
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
                        _testOutput = new StringBuilder();
                        process.OutputDataReceived += ProcessOnOutputDataReceived;
                        process.ErrorDataReceived += ProcessOnOutputDataReceived;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        if (_testResult.Status != Constants.TestExecutionStatus.Failed)
                        {
                            _testResult.Status = Constants.TestStatusList[process.ExitCode];
                        }

                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                    }
                });
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to Test Product {0}", exp);
                _testResult.Status = Constants.TestExecutionStatus.Timeout;
            }
            finally
            {
                _testResult.TestOutput = _testOutput?.ToString();
            }

            return _testResult;
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (args.Data != null && args.Data.EndsWith(CoverageExtension))
            {
                var coverageFile = args.Data.Trim();
                if (File.Exists(coverageFile))
                {
                    _testResult.CoveragePath = coverageFile;
                }
            }

            OnThresholdReached(args);

            if (KillProcessOnTestFail && args.Data != null &&
                (args.Data.StartsWith(FailedDuringExecution) ||
                 args.Data.StartsWith(ErrorDuringExecution)))
            {
                _testResult.Status = Constants.TestExecutionStatus.Failed;

                var process = (Process)sender;
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
        }

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            _testOutput.Append($"{arg.Data}\n");
        }
    }
}
