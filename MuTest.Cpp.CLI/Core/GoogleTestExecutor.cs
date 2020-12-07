using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MuTest.Cpp.CLI.Model;
using static MuTest.Core.Common.Constants;

namespace MuTest.Cpp.CLI.Core
{
    public class GoogleTestExecutor
    {
        private Process _currentProcess;
        private DateTime _currentDateTime;
        private const string TestCaseFilter = " --gtest_filter=";
        private const string ShuffleTests = " --gtest_shuffle";
        private const string FailedDuringExecution = "[  FAILED  ]";
        private static readonly object OutputDataReceivedLock = new object();
        private static readonly object TestTimeoutLock = new object();

        public bool KillProcessOnTestFail { get; set; } = false;

        public double TestTimeout { get; set; } = 15000;

        public bool EnableTestTimeout { get; set; }

        public string LogDir { get; set; }

        public TestExecutionStatus LastTestExecutionStatus { get; private set; }

        public Testsuites TestResult { get; private set; }

        public event EventHandler<string> OutputDataReceived;

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            OutputDataReceived?.Invoke(this, arg.Data);
        }

        public async Task ExecuteTests(string app, string filter)
        {
            try
            {
                _currentDateTime = DateTime.Now;
                var testResultFile = $@"""{LogDir}test_report_{_currentDateTime:yyyyMdhhmmss}.xml""";

                LastTestExecutionStatus = TestExecutionStatus.Success;
                TestResult = null;
                var methodBuilder = new StringBuilder(ShuffleTests);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    methodBuilder.Append($" {TestCaseFilter}")
                        .Append($"\"{filter}\"");
                }

                if (!string.IsNullOrWhiteSpace(LogDir))
                {
                    if (!Directory.Exists(LogDir))
                    {
                        Directory.CreateDirectory(LogDir);
                    }

                    methodBuilder.Append($" --gtest_output=xml:{testResultFile}");
                }

                var processInfo = new ProcessStartInfo(app)
                {
                    Arguments = methodBuilder.ToString(),
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                await Task.Run(() =>
                {
                    lock (TestTimeoutLock)
                    {
                        using (_currentProcess = new Process
                        {
                            StartInfo = processInfo,
                            EnableRaisingEvents = true
                        })
                        {
                            _currentProcess.OutputDataReceived += CurrentProcessOnOutputDataReceived;
                            _currentProcess.ErrorDataReceived += CurrentProcessOnOutputDataReceived;
                            _currentProcess.Start();
                            _currentProcess.BeginOutputReadLine();
                            _currentProcess.BeginErrorReadLine();
                            if (EnableTestTimeout)
                            {
                                _currentProcess.WaitForExit((int)TestTimeout);

                                if (!_currentProcess.HasExited)
                                {
                                    KillProcess(_currentProcess);
                                }
                            }
                            else
                            {
                                _currentProcess.WaitForExit();
                            }

                            if (LastTestExecutionStatus != TestExecutionStatus.Failed &&
                                LastTestExecutionStatus != TestExecutionStatus.Timeout)
                            {
                                LastTestExecutionStatus = TestStatusList.ContainsKey(_currentProcess.ExitCode)
                                    ? TestStatusList[_currentProcess.ExitCode]
                                    : _currentProcess.ExitCode < 0
                                        ? TestExecutionStatus.Failed
                                        : TestExecutionStatus.Timeout;
                            }

                            _currentProcess.OutputDataReceived -= CurrentProcessOnOutputDataReceived;
                            _currentProcess.ErrorDataReceived -= CurrentProcessOnOutputDataReceived;
                            GetTestResults(testResultFile);
                        }
                    }
                });

            }
            catch (Exception e)
            {
                Trace.TraceError("{0} - {1}", e.Message, e);
                throw;
            }
        }

        private void GetTestResults(string testLog)
        {
            try
            {
                var testFile = testLog.Replace(@"""", string.Empty);
                if (!File.Exists(testFile))
                {
                    return;
                }

                TestResult = testFile.LoadTestsFromFile();

            }
            catch (Exception exp)
            {
                TestResult = null;
                Trace.TraceError("Unknown Exception Occurred On Getting Test result {0}", exp);
            }
        }

        private void CurrentProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            lock (OutputDataReceivedLock)
            {
                OnThresholdReached(args);

                if (KillProcessOnTestFail && args.Data != null &&
                    args.Data.StartsWith(FailedDuringExecution))
                {
                    LastTestExecutionStatus = TestExecutionStatus.Failed;
                    KillProcess((Process)sender);
                }
            }
        }

        private static void KillProcess(Process process)
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }
    }
}