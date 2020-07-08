using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Common.Settings;
using MuTest.Core.Exceptions;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Core.Mutators;
using MuTest.Core.Testing;
using MuTest.Core.Utility;

namespace MuTest.Core.Common
{
    public class MutantAnalyzer : IMutantAnalyzer
    {
        private readonly IChalk _chalk;
        private readonly MuTestSettings _settings;
        private readonly int _useClassFilterTestsThreshold;
        private static readonly object _sync = new object();

        public IMutantExecutor MutantExecutor { get; set; }

        public int TotalMutants { get; private set; }

        public int MutantProgress { get; private set; }

        public double KilledThreshold { get; set; } = 1;

        public double SurvivedThreshold { get; set; } = 1;

        public bool EnableDiagnostics { get; set; }

        public int ConcurrentTestRunners { get; set; } = 4;

        public List<int> ExternalCoveredMutants { get; } = new List<int>();

        public string Specific { get; set; }

        public string RegEx { get; set; }

        public bool ExecuteAllTests { get; set; }

        public bool IncludeNestedClasses { get; set; }

        public bool UseExternalCodeCoverage { get; set; }

        public string ProcessWholeProject { get; set; }

        public string TestClass { get; set; }

        public bool IncludePartialClasses { get; set; }

        public string TestProject { get; set; }

        public string TestProjectLibrary { get; set; }

        public bool X64TargetPlatform { get; set; }

        public bool BuildInReleaseMode { get; set; }

        public string SourceProjectLibrary { get; set; }

        public int TestExecutionTime { get; set; } = -1;

        public bool NoCoverage { get; set; }

        public bool UseClassFilter { get; set; }

        public char ProgressIndicator { get; set; } = '*';

        public int MutantsPerLine { get; set; }

        public MutantAnalyzer(IChalk chalk, MuTestSettings settings)
        {
            _chalk = chalk ?? throw new ArgumentNullException(nameof(chalk));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _useClassFilterTestsThreshold = Convert.ToInt32(settings.UseClassFilterTestsThreshold);
        }

        public async Task<SourceClassDetail> Analyze(string sourceClass, string className, string sourceProject)
        {
            var testClaz = TestClass.GetClass();
            var source = new SourceClassDetail
            {
                ClassLibrary = SourceProjectLibrary,
                ClassProject = sourceProject,
                BuildInReleaseMode = BuildInReleaseMode,
                Claz = sourceClass.GetCodeFileContent().RootNode().ClassNode(className),
                FullName = className,
                FilePath = sourceClass,
                X64TargetPlatform = X64TargetPlatform,
                TestClaz = new TestClassDetail
                {
                    Claz = testClaz,
                    BuildInReleaseMode = BuildInReleaseMode,
                    ClassLibrary = TestProjectLibrary,
                    ClassProject = TestProject,
                    FilePath = TestClass,
                    FullName = testClaz.FullName(),
                    X64TargetPlatform = X64TargetPlatform
                }
            };
            _chalk.Yellow($"\nProcessing source class {source.DisplayName}");
            _chalk.Yellow($"\nTest class {source.TestClaz.DisplayName}");

            source.TestClaz.PartialClasses.Clear();
            source.TestClaz.PartialClasses.Add(new ClassDetail
            {
                Claz = source.TestClaz.Claz,
                FilePath = source.TestClaz.FilePath
            });

            var baseListSyntax = source.TestClaz.Claz.BaseList;
            if (baseListSyntax != null &&
                baseListSyntax.Types.Any())
            {
                foreach (var type in baseListSyntax.Types)
                {
                    var typeSyntax = type.Type;
                    var fileName = typeSyntax.ToString();
                    if (typeSyntax is GenericNameSyntax syntax)
                    {
                        fileName = syntax.Identifier.ValueText;
                    }

                    if (Path.GetDirectoryName(TestProject)
                        .FindFile($"{fileName}.cs")?
                        .GetCodeFileContent()
                        .RootNode() is CompilationUnitSyntax baseFile)
                    {
                        source.TestClaz.BaseClass = baseFile;
                    }
                }
            }

            if (IncludePartialClasses)
            {
                var testProjectFiles = Path.GetDirectoryName(TestClass).GetCSharpClassDeclarations();
                var testClassDetails = testProjectFiles
                    .SelectMany(cu => cu.CompilationUnitSyntax.DescendantNodes<ClassDeclarationSyntax>(),
                        (cu, classDeclarationSyntax) => new TestClassDetail
                        {
                            FullName = $"{cu.CompilationUnitSyntax.NameSpace()}.{classDeclarationSyntax.Identifier.Text}",
                            FilePath = cu.FileName,
                            TotalNumberOfMethods = classDeclarationSyntax.DescendantNodes<MethodDeclarationSyntax>().Count,
                            Claz = classDeclarationSyntax
                        }).Where(x => x.TotalNumberOfMethods > 0)
                    .OrderByDescending(x => x.TotalNumberOfMethods)
                    .ToList();

                foreach (var data in testClassDetails)
                {
                    if (!source.TestClaz.PartialClassNodesAdded &&
                        source.TestClaz.FullName == data.FullName &&
                        data.FilePath != source.TestClaz.FilePath)
                    {
                        source.TestClaz.PartialClasses.Add(data);
                    }
                }

                source.TestClaz.PartialClassNodesAdded = true;
            }

            await Initialization(source);

            return source;
        }

        private async Task InitItemSources(SourceClassDetail source)
        {
            await new MethodsInitializer
            {
                IncludeNestedClasses = IncludeNestedClasses
            }.FindMethods(source);
        }

        private async Task InitializeMutants(SourceClassDetail source)
        {
            _chalk.Default("\nInitialize Mutants...");
            var mutantAnalyzer = new MutantInitializer(source)
            {
                ExecuteAllTests = source.TestClaz.MethodDetails.Count > _useClassFilterTestsThreshold || ExecuteAllTests,
                MutantFilterRegEx = RegEx,
                SpecificFilterRegEx = Specific
            };

            if (UseExternalCodeCoverage)
            {
                mutantAnalyzer.ExecuteAllTests = true;
                mutantAnalyzer.MutantFilterRegEx = string.Empty;
                mutantAnalyzer.SpecificFilterRegEx = string.Empty;
                mutantAnalyzer.MutantsAtSpecificLines.AddRange(ExternalCoveredMutants);
            }

            await mutantAnalyzer.InitializeMutants(MutantOrchestrator.DefaultMutators);
        }

        private async Task Initialization(SourceClassDetail source)
        {
            var defaultMutants = MutantOrchestrator.GetDefaultMutants(source.Claz);

            await InitItemSources(source);
            if ((defaultMutants.Any() || string.IsNullOrWhiteSpace(ProcessWholeProject)) && !UseExternalCodeCoverage)
            {
                if (TestExecutionTime > -1)
                {
                    await FindTestExecutionTime(source);
                }

                await ExecuteTests(source);
            }

            await InitializeMutants(source);
            await AnalyzeMutant(source);
        }

        private async Task ExecuteTests(SourceClassDetail source)
        {
            _chalk.Default("\nExecuting Tests...");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testExecutor = new TestExecutor(_settings, source.TestClaz.ClassLibrary)
            {
                X64TargetPlatform = X64TargetPlatform,
                FullyQualifiedName = source.TestClaz.MethodDetails.Count > _useClassFilterTestsThreshold ||
                                     UseClassFilter || source.TestClaz.BaseClass != null
                    ? source.TestClaz.Claz.FullName()
                    : string.Empty,
                EnableCustomOptions = true,
                EnableLogging = true
            };
            testExecutor.OutputDataReceived += OutputData;

            await testExecutor.ExecuteTests(source.TestClaz.MethodDetails.ToList());
            source.NumberOfTests = Convert.ToInt32(testExecutor.TestResult?.ResultSummary?.Counters?.Total);

            if (testExecutor.LastTestExecutionStatus != Constants.TestExecutionStatus.Success)
            {
                throw new MuTestFailingTestException(log.ToString());
            }

            if (!NoCoverage)
            {
                _chalk.Green("\nCalculating Code Coverage...");

                if (testExecutor.CodeCoverage != null)
                {
                    var coverage = new CoverageAnalyzer();
                    coverage.FindCoverage(source, testExecutor.CodeCoverage);
                }

                _chalk.Green("\nCode Coverage is Loaded!\n");
            }

            testExecutor.OutputDataReceived -= OutputData;
        }

        private async Task FindTestExecutionTime(SourceClassDetail source)
        {
            _chalk.Default("\nFinding Tests Execution Time...");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testExecutor = new TestExecutor(_settings, source.TestClaz.ClassLibrary)
            {
                X64TargetPlatform = X64TargetPlatform,
                FullyQualifiedName = source.TestClaz.MethodDetails.Count > _useClassFilterTestsThreshold ||
                                     UseClassFilter || source.TestClaz.BaseClass != null
                    ? source.TestClaz.Claz.FullName()
                    : string.Empty,
                EnableLogging = true,
                EnableCustomOptions = false
            };

            testExecutor.OutputDataReceived += OutputData;

            await testExecutor.ExecuteTests(source.TestClaz.MethodDetails.ToList());

            testExecutor.OutputDataReceived -= OutputData;

            if (testExecutor.LastTestExecutionStatus != Constants.TestExecutionStatus.Success)
            {
                throw new MuTestFailingTestException(log.ToString());
            }

            if (testExecutor.TestResult?.Results?.UnitTestResult != null)
            {
                var tests = testExecutor.TestResult?.Results?.UnitTestResult;
                foreach (var test in tests)
                {
                    var executionTime = 0d;
                    if (test.Duration != null)
                    {
                        executionTime = TimeSpan.Parse(test.Duration).TotalMilliseconds;
                    }

                    if (executionTime <= TestExecutionTime)
                    {
                        source.TestExecutionTimes.Add(new TestExecutionTime(test.TestName, executionTime));
                    }
                    else
                    {
                        source.TestExecutionTimesAboveThreshold.Add(new TestExecutionTime(test.TestName, executionTime));
                    }
                }

                source.TestExecutionTimes.Sort((x1, x2) => x2.ExecutionTime.CompareTo(x1.ExecutionTime));
                source.TestExecutionTimesAboveThreshold.Sort((x1, x2) => x2.ExecutionTime.CompareTo(x1.ExecutionTime));

                foreach (var test in source.TestExecutionTimesAboveThreshold)
                {
                    _chalk.Red($"\n  {test.TestName} ({test.ExecutionTime}ms)");
                }

                foreach (var test in source.TestExecutionTimes)
                {
                    _chalk.Green($"\n  {test.TestName} ({test.ExecutionTime}ms)");
                }
            }
        }

        private async Task AnalyzeMutant(SourceClassDetail source)
        {
            _chalk.Default("\nPreparing Tests Files...\n");
            var directoryFactory = new TestDirectoryFactory(source)
            {
                NumberOfMutantsExecutingInParallel = ConcurrentTestRunners
            };
            directoryFactory.DeleteDirectories();
            await directoryFactory.PrepareDirectoriesAndFiles();

            _chalk.Default("\nRunning Mutation...\n");
            var mutantAnalyzer = new MutantExecutor(source, _settings)
            {
                NumberOfMutantsExecutingInParallel = ConcurrentTestRunners,
                EnableDiagnostics = EnableDiagnostics,
                SurvivedThreshold = 0.01,
                BaseAddress = _settings.ServiceAddress
            };

            if (!UseExternalCodeCoverage)
            {
                MutantExecutor = mutantAnalyzer;
                mutantAnalyzer.UseClassFilter = UseClassFilter ||
                                 source.TestClaz.MethodDetails.Count > _useClassFilterTestsThreshold ||
                                 source.TestClaz.BaseClass != null;
                mutantAnalyzer.SurvivedThreshold = SurvivedThreshold;
                mutantAnalyzer.KilledThreshold = KilledThreshold;
            }

            TotalMutants = source.MethodDetails.SelectMany(x => x.NotRunMutants).Count();
            MutantProgress = 0;
            mutantAnalyzer.MutantExecuted += MutantAnalyzerOnMutantExecuted;
            await mutantAnalyzer.ExecuteMutants();
            directoryFactory.DeleteDirectories();
        }

        private void MutantAnalyzerOnMutantExecuted(object sender, MutantEventArgs e)
        {
            lock (_sync)
            {
                var mutant = e.Mutant;
                var lineNumber = mutant.Mutation.OriginalNode.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                var status = $"{Environment.NewLine}Line: {lineNumber} - {mutant.ResultStatus.ToString()} - {mutant.Mutation.DisplayName}".PrintWithDateTimeSimple();

                switch (mutant.ResultStatus)
                {
                    case MutantStatus.Survived:
                        _chalk.Yellow(status);
                        break;
                    case MutantStatus.Timeout:
                        _chalk.Cyan(status);
                        break;
                    case MutantStatus.BuildError:
                        _chalk.Red(status);
                        break;
                    default:
                        _chalk.Green(status);
                        break;
                }

                if (EnableDiagnostics)
                {
                    _chalk.Red($"{Environment.NewLine}{e.BuildLog.ConvertToPlainText()}{Environment.NewLine}");
                    _chalk.Red($"{Environment.NewLine}{e.TestLog.ConvertToPlainText()}{Environment.NewLine}");
                }

                MutantProgress++;
                UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            if (TotalMutants == 0)
            {
                return;
            }

            var percentage = (int)100.0 * MutantProgress / TotalMutants;
            lock (_sync)
            {
                _chalk.Cyan(" [" + new string(ProgressIndicator, percentage / 2) + "] " + percentage + "%");
            }
        }
    }
}
