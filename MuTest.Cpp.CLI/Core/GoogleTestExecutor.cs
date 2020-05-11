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
        private const string ShuffleTests = " --gtest_shuffle";
        private const string FailedDuringExecution = "[  FAILED  ]";
        private static readonly object OutputDataReceivedLock = new object();
        private static readonly object TestTimeoutLock = new object();

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
            try
            {
                if (EnableTestTimeout)
                {
                    _timer = new Timer
                    {
                        Interval = TestTimeout,
                        AutoReset = false
                    };
                }

                LastTestExecutionStatus = TestExecutionStatus.Success;
                TestResult = null;
                var methodBuilder = new StringBuilder(ShuffleTests);

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    methodBuilder.Append($" {TestCaseFilter}")
                        .Append($"\"{filter}\"");
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
                            _timer.Elapsed += TimerOnElapsed;
                            _timer.Enabled = true;
                        }

                        _currentProcess.WaitForExit();

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
                    }
                });
            }
            catch (Exception e)
            {
                Trace.TraceError("{0} - {1}", e.Message, e);
                throw;
            }
            finally
            {
                _timer?.Dispose();
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (TestTimeoutLock)
            {
                LastTestExecutionStatus = TestExecutionStatus.Timeout;
                KillProcess(_currentProcess);
                _timer.Dispose();
            }
        }

        private void CurrentProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            lock (OutputDataReceivedLock)
            {
                if (EnableTestTimeout)
                {
                    _timer.Stop();
                    _timer.Enabled = false;
                    _timer.Elapsed -= TimerOnElapsed;
                    _timer.Close();
                    _timer = new Timer(TestTimeout)
                    {
                        Enabled = true,
                        AutoReset = false
                    };
                    _timer.Elapsed += TimerOnElapsed;
                }

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