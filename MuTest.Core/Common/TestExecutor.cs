using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model;
using MuTest.Core.Model.Service;
using MuTest.Core.Utility;
using Newtonsoft.Json;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common
{
    public class TestExecutor : ITestExecutor
    {
        private const string CommaSeparator = ",";
        private const string CoverageExtension = ".coverage";
        private const string TestCaseFilter = " /TestCaseFilter:";
        private const string FailedDuringExecution = "Failed ";
        private const string ErrorDuringExecution = "  X ";
        private static readonly object OutputDataReceivedLock = new object();

        private readonly MuTestSettings _settings;
        private readonly string _testClassLibrary;

        public string BaseAddress { get; set; }

        public CoverageDS CodeCoverage { get; private set; }

        public bool X64TargetPlatform { get; set; }

        public bool KillProcessOnTestFail { get; set; } = false;

        public bool EnableParallelTestExecution { get; set; } = false;

        public bool EnableCustomOptions { get; set; } = true;

        public bool EnableLogging { get; set; } = true;

        public TestExecutionStatus LastTestExecutionStatus { get; private set; }

        public string FullyQualifiedName { get; set; }

        private DateTime _currentDateTime;

        public TestRun TestResult { get; private set; }

        public event EventHandler<string> OutputDataReceived;

        public TestExecutor(MuTestSettings settings, string testClassLibrary)
        {
            if (string.IsNullOrWhiteSpace(testClassLibrary))
            {
                throw new ArgumentNullException(nameof(testClassLibrary));
            }

            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _testClassLibrary = testClassLibrary;
        }

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            OutputDataReceived?.Invoke(this, arg.Data);
        }

        public async Task ExecuteTests(IList<MethodDetail> selectedMethods)
        {
            if (selectedMethods == null && string.IsNullOrWhiteSpace(FullyQualifiedName))
            {
                throw new ArgumentNullException(nameof(selectedMethods));
            }

            _currentDateTime = DateTime.Now;
            LastTestExecutionStatus = TestExecutionStatus.Success;
            TestResult = null;
            var testResultFile = $@"""{_settings.TestsResultDirectory}report_{_currentDateTime:yyyyMdhhmmss}.trx""";
            var methodBuilder = new StringBuilder(_settings.Blame);

            if (EnableCustomOptions)
            {
                methodBuilder.Append(_settings.Options);
            }

            if (EnableParallelTestExecution)
            {
                methodBuilder.Append(_settings.ParallelTestExecution);
            }

            if (X64TargetPlatform)
            {
                methodBuilder.Append(" /Platform:x64");
            }

            methodBuilder
                .Append(_settings.SettingsOption)
                .Append($@"""{_settings.RunSettingsPath}""");

            if (EnableLogging)
            {
                methodBuilder.Append(_settings.LoggerOption.Replace("{0}", testResultFile));
            }

            if (string.IsNullOrWhiteSpace(FullyQualifiedName))
            {
                var methodDetails = selectedMethods.ToList();
                methodBuilder.Append(_settings.TestsOption);
                foreach (var method in methodDetails)
                {
                    methodBuilder
                        .Append($"{method.Method.Class().ClassName()}.{method.Method.MethodName()}")
                        .Append(CommaSeparator);
                }
            }
            else
            {
                methodBuilder.Append(TestCaseFilter)
                    .Append("\"")
                    .Append("FullyQualifiedName~")
                    .Append(FullyQualifiedName)
                    .Append("\"");
            }

            methodBuilder.Append($@" ""{_testClassLibrary}""");

            if (string.IsNullOrWhiteSpace(BaseAddress))
            {
                var processInfo = new ProcessStartInfo(_settings.VSTestConsolePath)
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

                        if (LastTestExecutionStatus != TestExecutionStatus.Failed)
                        {
                            LastTestExecutionStatus = TestStatusList[process.ExitCode];
                        }

                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                    }
                });

                if (EnableLogging)
                {
                    GetTestResults(testResultFile);
                }
            }
            else
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(
                            new MediaTypeWithQualityHeaderValue("application/json"));
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>(nameof(TestInput.Arguments), methodBuilder.ToString()),
                            new KeyValuePair<string, string>(nameof(TestInput.KillProcessOnTestFail), KillProcessOnTestFail.ToString())
                        });

                        client.Timeout = Timeout.InfiniteTimeSpan;
                        var response = await client.PostAsync($"{BaseAddress}api//mutest/test", content);
                        if (response.IsSuccessStatusCode)
                        {
                            OutputDataReceived?.Invoke(this, $"MuTest Test Service is executing tests at {BaseAddress}\n");
                            var responseData = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<TestResult>(responseData);
                            OutputDataReceived?.Invoke(this, result.TestOutput);
                            LastTestExecutionStatus = result.Status;

                            if (!string.IsNullOrWhiteSpace(result.CoveragePath) && File.Exists(result.CoveragePath))
                            {
                                using (CoverageInfo info = CoverageInfo.CreateFromFile(
                                    result.CoveragePath,
                                    new[]
                                    {
                                        Path.GetDirectoryName(_testClassLibrary)
                                    },
                                    new[]
                                    {
                                        Path.GetDirectoryName(_testClassLibrary)
                                    }))
                                {
                                    CodeCoverage = info.BuildDataSet();
                                }
                            }
                        }
                        else
                        {
                            LastTestExecutionStatus = TestExecutionStatus.Timeout;
                            Trace.TraceError("MuTest Test Service not Found at {0}", BaseAddress);
                            OutputDataReceived?.Invoke(this, $"MuTest Test Service is not running at {BaseAddress}\n");
                        }
                    }

                    if (EnableLogging)
                    {
                        GetTestResults(testResultFile);
                    }
                }
                catch (Exception exp)
                {
                    Trace.TraceError("MuTest Test Service not Found at {0}. Unable to Test Product {1}", BaseAddress, exp);
                    OutputDataReceived?.Invoke(this, $"MuTest Test Service is not running at {BaseAddress}\n");
                    LastTestExecutionStatus = TestExecutionStatus.Failed;
                }
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            lock (OutputDataReceivedLock)
            {
                if (args.Data != null && args.Data.EndsWith(CoverageExtension))
                {
                    var coverageFile = args.Data.Trim();
                    if (File.Exists(coverageFile))
                    {
                        using (CoverageInfo info = CoverageInfo.CreateFromFile(
                            coverageFile,
                            new[]
                            {
                                Path.GetDirectoryName(_testClassLibrary)
                            },
                            new[]
                            {
                                Path.GetDirectoryName(_testClassLibrary)
                            }))
                        {
                            CodeCoverage = info.BuildDataSet();
                        }
                    }
                }

                OnThresholdReached(args);

                if (KillProcessOnTestFail && args.Data != null &&
                    (args.Data.StartsWith(FailedDuringExecution) ||
                     args.Data.StartsWith(ErrorDuringExecution)))
                {
                    LastTestExecutionStatus = TestExecutionStatus.Failed;

                    var process = (Process) sender;
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
            }
        }

        public string PrintTestResult(TestRun testRun)
        {
            if (testRun == null)
            {
                return null;
            }

            var testResultBuilder = new StringBuilder(PreStart);

            if (testRun.Results != null)
            {
                foreach (var testRunResult in testRun.Results.UnitTestResult)
                {
                    var executionTime = 0d;
                    if (testRunResult.Duration != null)
                    {
                        executionTime = TimeSpan.Parse(testRunResult.Duration).TotalMilliseconds;
                    }

                    var color = testRunResult.Outcome == PassedOutCome
                        ? Colors.Green
                        : testRunResult.Outcome == FailedOutCome
                            ? Colors.Red
                            : Colors.Gold;

                    testResultBuilder.AppendLine($"{testRunResult.Outcome} {testRunResult.TestName} Execution Time: <b>{executionTime}ms</b>".Print(color: color));
                }
            }

            if (testRun.ResultSummary != null)
            {
                var counters = testRun.ResultSummary.Counters;
                testResultBuilder.AppendLine()
                    .AppendLine(
                        $@"Total={counters.Total} Executed={counters.Executed} Passed={counters.Passed} Failed={counters.Failed}"
                            .Print(color: DefaultColor))
                    .Append(PreEnd);
            }

            var result = testResultBuilder.ToString();

            return result;
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

                using (var fileStream = File.Open(testFile, FileMode.Open))
                {
                    var xmlSerializer = new XmlSerializer(typeof(TestRun));
                    var testRun = (TestRun)xmlSerializer.Deserialize(fileStream);
                    TestResult = testRun;
                    fileStream.Close();
                }
            }
            catch (Exception exp)
            {
                TestResult = null;
                Trace.TraceError("Unknown Exception Occurred On Getting Test result {0}", exp);
            }
        }
    }
}