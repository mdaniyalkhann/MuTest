using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Exceptions;
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

        private IChalk _chalk;
        private MuTestOptions _options;
        private Stopwatch _stopwatch;
        private CppBuildContext _context;

        public async Task RunMutationTest(MuTestOptions options)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _chalk = new Chalk();
            _options = options;

            _chalk.Default("\nPreparing Required Files...\n");

            _context = new CppDirectoryFactory
            {
                NumberOfMutantsExecutingInParallel = _options.ConcurrentTestRunners
            }.PrepareTestDirectories(
                options.TestClass,
                options.SourceClass,
                options.TestProject,
                options.TestSolution);

            if (_context.TestContexts.Any())
            {
                await ExecuteBuild();
                await ExecuteTests();

                var mutant = MutantOrchestrator.GetDefaultMutants(_options.SourceClass);
                foreach (var defaultMutant in mutant)
                {
                    _chalk.Green($"\n{defaultMutant.Mutation.DisplayName}\n");
                }
            }
        }

        private async Task ExecuteBuild()
        {
            _chalk.Default("\nBuilding Solution...\n");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testCodeBuild = new CppBuildExecutor(
                VsTestConsoleSettings,
                _context.TestSolution.FullName,
                Path.GetFileNameWithoutExtension(_options.TestProject))
            {
                Configuration = _options.Configuration,
                EnableLogging = _options.EnableDiagnostics,
                IntDir = _context.IntDir,
                IntermediateOutputPath = _context.IntermediateOutputPath,
                OutDir = _context.OutDir,
                OutputPath = _context.OutputPath,
                Platform = _options.Platform,
                QuietWithSymbols = true
            };

            testCodeBuild.OutputDataReceived += OutputData;

            await testCodeBuild.ExecuteBuild();

            testCodeBuild.OutputDataReceived -= OutputData;

            if (testCodeBuild.LastBuildStatus == Constants.BuildExecutionStatus.Failed)
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
            var testExecutor = new GoogleTestExecutor();
      
            testExecutor.OutputDataReceived += OutputData;
            var projectDirectory = Path.GetDirectoryName(_options.TestProject);
            var projectName = Path.GetFileNameWithoutExtension(_options.TestProject);

            await testExecutor.ExecuteTests(
                $"{projectDirectory}/{_context.OutDir}{projectName}.exe", 
                $"{Path.GetFileNameWithoutExtension(_context.TestContexts.First().TestClass.Name)}.*");

            if (testExecutor.LastTestExecutionStatus != Constants.TestExecutionStatus.Success)
            {
                throw new MuTestFailingTestException(log.ToString());
            }

            testExecutor.OutputDataReceived -= OutputData;
        }
    }
}