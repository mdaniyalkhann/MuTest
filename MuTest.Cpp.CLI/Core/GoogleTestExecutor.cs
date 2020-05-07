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
        private Timer _timerChildProcesses;
        private const string TestCaseFilter = " --gtest_filter=";
        private const string ShuffleTests = " --gtest_shuffle";
        private const string FailedDuringExecution = "[  FAILED  ]";
        private static readonly object TimerLock = new object();
        private static readonly object OutputDataReceivedLock = new object();

        public bool KillProcessOnTestFail { get; set; } = false;

        public double TestTimeout { get; set; } = 10000;

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
                Interval = TestTimeout,
                AutoReset = false
            };

            _timerChildProcesses = new Timer
            {
                Interval = 1000,
                AutoReset = false
            };

            LastTestExecutionStatus = TestExecutionStatus.Success;
            TestResult = null;
            var methodBuilder = new StringBuilder(ShuffleTests);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                methodBuilder.Append($" {TestCaseFilter}")
                    .Append($"\"{filter}\"");
            }

            if (KillProcessOnTestFail)
            {
                methodBuilder.Append(" --gtest_catch_exceptions=0");
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

                        _timerChildProcesses.Elapsed += ChildProcessTimerOnElapsed;
                        _timerChildProcesses.Enabled = true;
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

        private void ChildProcessTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (TimerLock)
            {
                if (_currentProcess != null)
                {
                    foreach (ProcessThread thread in _currentProcess.Threads)
                    {
                        if (thread.ThreadState == ThreadState.Wait
                            && thread.WaitReason == ThreadWaitReason.UserRequest)
                        {
                            LastTestExecutionStatus = TestExecutionStatus.Failed;
                            KillProcess(_currentProcess);
                        }
                    }
                }
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            LastTestExecutionStatus = TestExecutionStatus.Timeout;
            ChildProcessTimerOnElapsed(sender, e);
            KillProcess(_currentProcess);
            _timer.Dispose();
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
                    KillProcess((Process) sender);
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