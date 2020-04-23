using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Exceptions;
using MuTest.Core.Mutants;
using MuTest.Core.Testing;
using MuTest.Cpp.CLI.Core;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
{
    public class MuTestRunner : IMuTestRunner
    {
        public static readonly VSTestConsoleSettings VsTestConsoleSettings = VSTestConsoleSettingsSection.GetSettings();

        private readonly IChalk _chalk;
        private readonly ICppDirectoryFactory _directoryFactory;
        private MuTestOptions _options;
        private Stopwatch _stopwatch;
        private CppBuildContext _context;
        private int _totalMutants;
        private int _mutantProgress;
        private static readonly object Sync = new object();
        private CppClass _cppClass;

        public ICppMutantExecutor MutantsExecutor { get; private set; }

        public MuTestRunner(IChalk chalk, ICppDirectoryFactory directoryFactory)
        {
            _chalk = chalk;
            _directoryFactory = directoryFactory;
        }

        public async Task RunMutationTest(MuTestOptions options)
        {
            try
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _options = options;

                _chalk.Default("\nPreparing Required Files...\n");

                _directoryFactory.NumberOfMutantsExecutingInParallel = _options.ConcurrentTestRunners;

                _cppClass = new CppClass
                {
                    Configuration = _options.Configuration,
                    SourceClass = _options.SourceClass,
                    Platform = _options.Platform,
                    TestClass = _options.TestClass,
                    TestProject = _options.TestProject,
                    SourceHeader = _options.SourceHeader,
                    TestSolution = _options.TestSolution
                };


                _context = !_options.InIsolation
                    ? _directoryFactory.PrepareTestFiles(_cppClass)
                    : _directoryFactory.PrepareSolutionFiles(_cppClass);

                if (_context.TestContexts.Any())
                {
                    await ExecuteBuild();
                    await ExecuteTests();

                    _chalk.Default("\nRunning Mutation...\n");


                    _cppClass.Mutants.AddRange(
                        CppMutantOrchestrator.GetDefaultMutants(_options.SourceClass));

                    MutantsExecutor = new CppMutantExecutor(_cppClass, _context, VsTestConsoleSettings)
                    {
                        EnableDiagnostics = _options.EnableDiagnostics,
                        KilledThreshold = _options.KilledThreshold,
                        SurvivedThreshold = _options.SurvivedThreshold,
                        NumberOfMutantsExecutingInParallel = _options.ConcurrentTestRunners
                    };

                    _totalMutants = _cppClass.Mutants.Count;
                    _mutantProgress = 0;
                    MutantsExecutor.MutantExecuted += MutantAnalyzerOnMutantExecuted;
                    await MutantsExecutor.ExecuteMutants();

                }
            }
            finally
            {
                _directoryFactory.DeleteTestFiles(_context);
            }
        }

        private void MutantAnalyzerOnMutantExecuted(object sender, CppMutantEventArgs e)
        {
            lock (Sync)
            {
                var mutant = e.Mutant;
                var lineNumber = mutant.Mutation.LineNumber;
                var status = $"{Environment.NewLine}Line: {lineNumber} - {mutant.ResultStatus.ToString()} - {mutant.Mutation.DisplayName}".PrintWithDateTimeSimple();

                if (mutant.ResultStatus == MutantStatus.Survived)
                {
                    _chalk.Yellow($"{status}{Environment.NewLine}");
                }
                else if (mutant.ResultStatus == MutantStatus.BuildError)
                {
                    _chalk.Red($"{status}{Environment.NewLine}");
                }
                else if (mutant.ResultStatus == MutantStatus.Timeout)
                {
                    _chalk.Cyan($"{status}{Environment.NewLine}");
                }
                else
                {
                    _chalk.Green($"{status}{Environment.NewLine}");
                }

                if (_options.EnableDiagnostics)
                {
                    _chalk.Red($"{e.BuildLog.ConvertToPlainText()}{Environment.NewLine}");
                    _chalk.Red($"{e.TestLog.ConvertToPlainText()}{Environment.NewLine}");
                }

                _mutantProgress++;
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            if (_totalMutants == 0)
            {
                return;
            }

            var percentage = (int)100.0 * _mutantProgress / _totalMutants;
            lock (Sync)
            {
                _chalk.Cyan(" [" + new string('*', percentage / 2) + "] " + percentage + "%");
            }
        }

        private async Task ExecuteBuild()
        {
            _chalk.Default("\nBuilding Solution...\n");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testCodeBuild = new CppBuildExecutor(
                VsTestConsoleSettings,
                string.Format(_context.TestSolution.FullName, 0),
                Path.GetFileNameWithoutExtension(_options.TestProject))
            {
                Configuration = _options.Configuration,
                EnableLogging = _options.EnableDiagnostics,
                IntDir = string.Format(_context.IntDir, 0),
                IntermediateOutputPath = string.Format(_context.IntermediateOutputPath, 0),
                OutDir = string.Format(_context.OutDir, 0),
                OutputPath = string.Format(_context.OutputPath, 0),
                Platform = _options.Platform,
                QuietWithSymbols = true
            };

            testCodeBuild.OutputDataReceived += OutputData;

            await testCodeBuild.ExecuteBuild();

            testCodeBuild.OutputDataReceived -= OutputData;

            if (testCodeBuild.LastBuildStatus == Constants.BuildExecutionStatus.Failed)
            {
                _chalk.Yellow("\nBuild Failed...Preparing new solution files\n");
                _directoryFactory.DeleteTestFiles(_context);
                _context = _directoryFactory.PrepareSolutionFiles(_cppClass);

                testCodeBuild = new CppBuildExecutor(
                    VsTestConsoleSettings,
                    string.Format(_context.TestSolution.FullName, 0),
                    Path.GetFileNameWithoutExtension(_options.TestProject))
                {
                    Configuration = _options.Configuration,
                    EnableLogging = _options.EnableDiagnostics,
                    IntDir = string.Format(_context.IntDir, 0),
                    IntermediateOutputPath = string.Format(_context.IntermediateOutputPath, 0),
                    OutDir = string.Format(_context.OutDir, 0),
                    OutputPath = string.Format(_context.OutputPath, 0),
                    Platform = _options.Platform,
                    QuietWithSymbols = true
                };

                await testCodeBuild.ExecuteBuild();
                if (testCodeBuild.LastBuildStatus == Constants.BuildExecutionStatus.Failed)
                {
                    throw new MuTestFailingBuildException(log.ToString());
                }
            }

            _chalk.Green("\nBuild Succeeded!");
        }

        private async Task ExecuteTests()
        {
            _chalk.Default("\nExecuting Tests...");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testExecutor = new GoogleTestExecutor();

            testExecutor.OutputDataReceived += OutputData;
            var projectDirectory = Path.GetDirectoryName(_options.TestProject);
            var projectName = Path.GetFileNameWithoutExtension(_options.TestProject);

            await testExecutor.ExecuteTests(
                $"{projectDirectory}/{string.Format(_context.OutDir, 0)}{projectName}.exe",
                $"{Path.GetFileNameWithoutExtension(_context.TestContexts.First().TestClass.Name)}.*");

            if (testExecutor.LastTestExecutionStatus != Constants.TestExecutionStatus.Success)
            {
                throw new MuTestFailingTestException(log.ToString());
            }

            testExecutor.OutputDataReceived -= OutputData;
        }
    }
}