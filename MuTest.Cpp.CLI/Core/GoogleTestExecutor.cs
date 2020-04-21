using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using MuTest.Core.Model;
using static MuTest.Core.Common.Constants;

namespace MuTest.Cpp.CLI.Core
{
    public class GoogleTestExecutor
    {
        private Process _currentProcess;
        private Timer _timer;
        private const string TestCaseFilter = " --gtest_filter=";
        private const string FailedDuringExecution = "[  FAILED  ]";

        public bool KillProcessOnTestFail { get; set; } = false;

        public double TestTimeout { get; set; } = 15000;

        public bool EnableTestTimeout { get; set; }

        public TestExecutionStatus LastTestExecutionStatus { get; private set; }

        public TestRun TestResult { get; private set; }

        public event EventHandler<string> OutputDataReceived;

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            OutputDataReceived?.Invoke(this, arg.Data);
        }

        public async Task ExecuteTests(string app, string filter)
        {
            _timer = new Timer
            {
                Interval = TestTimeout
            };

            LastTestExecutionStatus = TestExecutionStatus.Success;
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
                using (_currentProcess = new Process
                {
                    StartInfo = processInfo,
                    EnableRaisingEvents = true
                })
                {
                    _timer.Elapsed += TimerOnElapsed;
                    _timer.Enabled = true;
                    _currentProcess.OutputDataReceived += CurrentProcessOnOutputDataReceived;
                    _currentProcess.ErrorDataReceived += CurrentProcessOnOutputDataReceived;
                    _currentProcess.Start();
                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();
                    _currentProcess.WaitForExit();

                    if (LastTestExecutionStatus != TestExecutionStatus.Failed && 
                        LastTestExecutionStatus != TestExecutionStatus.Timeout)
                    {
                        LastTestExecutionStatus = TestStatusList[_currentProcess.ExitCode];
                    }

                    _currentProcess.OutputDataReceived -= CurrentProcessOnOutputDataReceived;
                    _currentProcess.ErrorDataReceived -= CurrentProcessOnOutputDataReceived;
                }
            });
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            LastTestExecutionStatus = TestExecutionStatus.Timeout;

            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _currentProcess.Kill();
            }

            _timer.Dispose();
        }

        private void CurrentProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            if (EnableTestTimeout)
            {
                _timer.Elapsed -= TimerOnElapsed;
                _timer?.Dispose();
                _timer = new Timer(TestTimeout)
                {
                    Enabled = true
                };
                _timer.Elapsed += TimerOnElapsed;
            }

            OnThresholdReached(args);

            if (KillProcessOnTestFail && args.Data != null &&
                args.Data.StartsWith(FailedDuringExecution))
            {
                LastTestExecutionStatus = TestExecutionStatus.Failed;

                var process = (Process)sender;
                if (process != null && !process.HasExited)
                {
                    process.Kill();
                }
            }
        }
    }
}