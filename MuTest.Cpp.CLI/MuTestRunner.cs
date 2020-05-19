using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Exceptions;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Core.Testing;
using MuTest.Cpp.CLI.Core;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;
using MuTest.Cpp.CLI.Options;
using MuTest.Cpp.CLI.Utility;
using Newtonsoft.Json;
using static MuTest.Core.Common.Constants;

namespace MuTest.Cpp.CLI
{
    public class MuTestRunner : IMuTestRunner
    {
        public static readonly MuTestSettings MuTestSettings = MuTestSettingsSection.GetSettings();

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
                    Target = _options.Target,
                    SourceHeader = _options.SourceHeader,
                    TestSolution = _options.TestSolution,
                    IncludeBuildEvents = _options.IncludeBuildEvents
                };

                _context = !_options.InIsolation
                    ? _directoryFactory.PrepareTestFiles(_cppClass)
                    : _directoryFactory.PrepareSolutionFiles(_cppClass);

                if (_context.TestContexts.Any())
                {
                    await ExecuteBuild();
                    await ExecuteTests();

                    if (!_options.DisableBuildOptimization)
                    {
                        _context.EnableBuildOptimization = true;
                    }

                    _chalk.Default("\nRunning Mutation Analysis...\n");


                    _cppClass.Mutants.AddRange(
                        CppMutantOrchestrator.GetDefaultMutants(_options.SourceClass, _options.SpecificLines));

                    if (_cppClass.CoveredLineNumbers.Any())
                    {
                        foreach (var mutant in _cppClass.Mutants)
                        {
                            if (_cppClass.CoveredLineNumbers.All(x => x != mutant.Mutation.LineNumber))
                            {
                                mutant.ResultStatus = MutantStatus.NotCovered;
                            }
                        }
                    }

                    _chalk.Default($"\nNumber of Mutants: {_cppClass.Mutants.Count}\n");

                    if (_cppClass.Mutants.Any())
                    {
                        MutantsExecutor = new CppMutantExecutor(_cppClass, _context, MuTestSettings)
                        {
                            EnableDiagnostics = _options.EnableDiagnostics,
                            KilledThreshold = _options.KilledThreshold,
                            SurvivedThreshold = _options.SurvivedThreshold,
                            NumberOfMutantsExecutingInParallel = _options.ConcurrentTestRunners
                        };

                        _totalMutants = _cppClass.NotRunMutants.Count;
                        _mutantProgress = 0;
                        MutantsExecutor.MutantExecuted += MutantAnalyzerOnMutantExecuted;
                        await MutantsExecutor.ExecuteMutants();
                        GenerateReports();
                    }

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
                MuTestSettings,
                string.Format(_context.TestSolution.FullName, 0),
                _cppClass.Target)
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

            if (!_options.IncludeBuildEvents)
            {
                string.Format(_context.TestProject.FullName, 0).RemoveBuildEvents();
            }

            testCodeBuild.OutputDataReceived += OutputData;

            await testCodeBuild.ExecuteBuild();

            testCodeBuild.OutputDataReceived -= OutputData;

            if (testCodeBuild.LastBuildStatus == BuildExecutionStatus.Failed && !_options.InIsolation)
            {
                _chalk.Yellow("\nBuild Failed...Preparing new solution files\n");
                _directoryFactory.DeleteTestFiles(_context);
                _context = _directoryFactory.PrepareSolutionFiles(_cppClass);

                testCodeBuild = new CppBuildExecutor(
                    MuTestSettings,
                    string.Format(_context.TestSolution.FullName, 0),
                    _cppClass.Target)
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
            }

            if (testCodeBuild.LastBuildStatus == BuildExecutionStatus.Failed)
            {
                throw new MuTestFailingBuildException(log.ToString());
            }

            _chalk.Green("\nBuild Succeeded!");
        }

        private async Task ExecuteTests()
        {
            _chalk.Default("\nExecuting Tests...");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testExecutor = new GoogleTestExecutor
            {
                LogDir = MuTestSettings.TestsResultDirectory
            };

            testExecutor.OutputDataReceived += OutputData;
            var projectDirectory = Path.GetDirectoryName(_options.TestProject);
            var projectName = Path.GetFileNameWithoutExtension(_options.TestProject);
            var projectNameFromTestContext = string.Format(Path.GetFileNameWithoutExtension(_context.TestProject.Name), 0);

            var app = $"{projectDirectory}\\{string.Format(_context.OutDir.TrimEnd('/'), 0)}\\{projectName}.exe";

            if (!File.Exists(app))
            {
                app = $"{projectDirectory}/{string.Format(_context.OutDir, 0)}{projectNameFromTestContext}.exe";
            }

            if (!File.Exists(app))
            {
                throw new MuTestFailingTestException($"Unable to find google tests at path {app}");
            }

            var cppTestContext = _context.TestContexts.First();
            var filter = $"{Path.GetFileNameWithoutExtension(cppTestContext.TestClass.Name)}*";
            await testExecutor.ExecuteTests(app, filter);

            if (testExecutor.TestResult != null)
            {
                _cppClass.NumberOfTests = Convert.ToInt32(testExecutor.TestResult.Tests);
                _cppClass.NumberOfDisabledTests = Convert.ToInt32(testExecutor.TestResult.Disabled);

                _chalk.Default($"\n\nNumber of Tests: {_cppClass.NumberOfTests}\n");

                _cppClass.Tests.AddRange(testExecutor
                    .TestResult
                    .Testsuite
                    .SelectMany(x => x.Testcase)
                    .Select(x => new Test
                    {
                        Name = $"{x.Classname.Replace("_mutest_test_0", string.Empty)}.{x.Name}",
                        ExecutionTime = Convert.ToDouble(x.Time)
                    }));
            }

            if (testExecutor.LastTestExecutionStatus != TestExecutionStatus.Success)
            {
                throw new MuTestFailingTestException(log.ToString());
            }

            _chalk.Default("\nCalculating Code Coverage...\n");
            var coverageExecutor = new OpenCppCoverageExecutor(MuTestSettings.OpenCppCoveragePath, MuTestSettings.TestsResultDirectory);

            await coverageExecutor
                .GenerateCoverage(
                    cppTestContext.SourceClass.DirectoryName, app, filter);

            if (coverageExecutor.CoverageReport != null)
            {
                foreach (var package in coverageExecutor.CoverageReport.Packages.Package)
                {
                    foreach (var packageClass in package.Classes.Class)
                    {
                        if (cppTestContext.SourceClass.FullName.EndsWith(packageClass.Filename, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var coveredLines = (uint)packageClass.Lines.Line.Count(x => x.Hits > 0);
                            var totalLines = (uint)packageClass.Lines.Line.Count;

                            if (totalLines == 0)
                            {
                                totalLines = 1;
                            }

                            _chalk.Yellow($"\nCode Coverage for Class {Path.GetFileName(_cppClass.SourceClass)} is {decimal.Divide(coveredLines, totalLines):P} ({coveredLines}/{totalLines})\n");
                            _cppClass.Coverage = Coverage.Create(coveredLines, totalLines - coveredLines, 0, 0);

                            var factor = _context.UseMultipleSolutions || !_context.NamespaceAdded
                                ? 0u
                                : 3u;

                            for (var index = 0; index < packageClass.Lines.Line.Count; index++)
                            {
                                var line = packageClass.Lines.Line[index];
                                var currentLineNumber = Convert.ToUInt32(line.Number);
                                if (line.Hits > 0)
                                {
                                    _cppClass.CoveredLineNumbers.Add(currentLineNumber - factor);

                                    if (index + 1 < packageClass.Lines.Line.Count)
                                    {
                                        var nextLineNumber = Convert.ToUInt32(packageClass.Lines.Line[index + 1].Number);
                                        for (var lineNumber = currentLineNumber; lineNumber < nextLineNumber; lineNumber++)
                                        {
                                            _cppClass.CoveredLineNumbers.Add(lineNumber - factor);
                                        }
                                    }
                                }
                            }

                            break;
                        }
                    }
                }
            }

            testExecutor.OutputDataReceived -= OutputData;
        }

        private void GenerateReports()
        {
            var consoleBuilder = new StringBuilder();
            if (_cppClass != null)
            {
                MutantsExecutor.PrintMutatorSummary(consoleBuilder, _cppClass.Mutants);
                MutantsExecutor.PrintClassSummary(_cppClass, consoleBuilder);
                consoleBuilder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10;\">");
                consoleBuilder.AppendLine("Execution Time: ".PrintImportantWithLegend());
                consoleBuilder.Append($"{_stopwatch.Elapsed}".PrintWithPreTagWithMarginImportant());
                consoleBuilder.AppendLine("</fieldset>");

                _chalk.Default($"{Environment.NewLine}{consoleBuilder.ToString().ConvertToPlainText()}{Environment.NewLine}");

                _cppClass.ExecutionTime = _stopwatch.ElapsedMilliseconds;
                var builder = new StringBuilder(HtmlTemplate);
                builder.Append(MutantsExecutor?.LastExecutionOutput.PrintImportant() ?? string.Empty);
                MutantsExecutor?.PrintMutatorSummary(builder, _cppClass.Mutants);
                MutantsExecutor?.PrintClassSummary(_cppClass, builder);
                builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10;\">");
                builder.AppendLine("Execution Time: ".PrintImportantWithLegend());
                builder.Append($"{_stopwatch.Elapsed}".PrintWithPreTagWithMarginImportant());
                builder.AppendLine("</fieldset>");

                var fileName = Path.GetFileNameWithoutExtension(_cppClass.SourceClass)?.Replace(".", "_");
                CreateHtmlReport(builder, fileName);
                CreateJsonReport(fileName,
                    new JsonOptions
                    {
                        Options = _options,
                        Result = _cppClass
                    });
            }
        }

        private void CreateJsonReport<T>(string fileName, T output)
        {
            if (!string.IsNullOrWhiteSpace(_options.JsonOutputPath))
            {
                var outputPath = _options.JsonOutputPath.Replace(SourceClassPlaceholder, fileName);
                var file = new FileInfo(outputPath);
                if (file.Exists)
                {
                    file.Delete();
                }

                var directoryName = Path.GetDirectoryName(file.FullName);
                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                file.Create().Close();
                File.WriteAllText(outputPath, JsonConvert.SerializeObject(output, Formatting.Indented));

                _chalk.Green($"\nYour json report has been generated at: \n {file.FullName} \n");
            }
        }

        private void CreateHtmlReport(StringBuilder builder, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(_options.HtmlOutputPath))
            {
                var outputPath = _options.HtmlOutputPath.Replace(SourceClassPlaceholder, fileName);
                var file = new FileInfo(outputPath);
                if (file.Exists)
                {
                    file.Delete();
                }

                var directoryName = Path.GetDirectoryName(file.FullName);
                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                file.Create().Close();
                File.WriteAllText(outputPath, builder.ToString());

                _chalk.Green($"\nYour html report has been generated at: \n {file.FullName} \nYou can open it in your browser of choice. \n");
            }
        }
    }
}