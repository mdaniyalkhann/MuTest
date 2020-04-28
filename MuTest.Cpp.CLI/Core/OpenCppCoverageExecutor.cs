using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Model;
using MuTest.Cpp.CLI.Model;

namespace MuTest.Cpp.CLI.Core
{
    public class OpenCppCoverageExecutor
    {
        private readonly string _openCppCoveragePath;
        private readonly string _testResultsDirectory;
        private Process _currentProcess;
        private string _coverageFile;
        private const string TestCaseFilter = " --gtest_filter=";

        public CoberturaCoverageReport CoverageReport { get; set; }


        public TestRun TestResult { get; private set; }

        public event EventHandler<string> OutputDataReceived;

        protected virtual void OnThresholdReached(DataReceivedEventArgs arg)
        {
            OutputDataReceived?.Invoke(this, arg.Data);
        }

        public OpenCppCoverageExecutor(string openCppCoveragePath, string testResultsDirectory)
        {
            if (string.IsNullOrWhiteSpace(openCppCoveragePath))
            {
                throw new ArgumentNullException(nameof(openCppCoveragePath));
            }

            if (string.IsNullOrWhiteSpace(testResultsDirectory))
            {
                throw new ArgumentNullException(nameof(testResultsDirectory));
            }

            _openCppCoveragePath = openCppCoveragePath;
            _testResultsDirectory = testResultsDirectory;
        }

        public async Task GenerateCoverage(string sources, string app, string filter)
        {
            _coverageFile = $@"{_testResultsDirectory}report_{DateTime.Now:yyyyMdhhmmss}.xml";
            CoverageReport = null;
            TestResult = null;
            var methodBuilder = new StringBuilder(" --sources ")
                .Append(sources)
                .Append(" --export_type=cobertura:")
                .Append(_coverageFile)
                .Append(" -- ")
                .Append(app);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                methodBuilder.Append($" {TestCaseFilter}")
                    .Append($"\"{filter}\"");
            }

            var processInfo = new ProcessStartInfo(_openCppCoveragePath)
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
                    _currentProcess.OutputDataReceived += CurrentProcessOnOutputDataReceived;
                    _currentProcess.ErrorDataReceived += CurrentProcessOnOutputDataReceived;
                    _currentProcess.Start();
                    _currentProcess.BeginOutputReadLine();
                    _currentProcess.BeginErrorReadLine();
                    _currentProcess.WaitForExit();

                    if (File.Exists(_coverageFile))
                    {
                        CoverageReport = _coverageFile.LoadFromFile();
                    }

                    _currentProcess.OutputDataReceived -= CurrentProcessOnOutputDataReceived;
                    _currentProcess.ErrorDataReceived -= CurrentProcessOnOutputDataReceived;
                }
            });
        }

        private void CurrentProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnThresholdReached(args);
        }
    }
}