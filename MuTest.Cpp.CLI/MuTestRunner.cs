using System.Diagnostics;
using System.Threading.Tasks;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model;
using MuTest.Core.Testing;
using MuTest.Cpp.CLI.Core;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
{
    public class MuTestRunner : IMuTestRunner
    {
        public static readonly VSTestConsoleSettings VsTestConsoleSettings = VSTestConsoleSettingsSection.GetSettings();

        private SourceClassDetail _source;
        private IChalk _chalk;
        private MuTestOptions _options;
        private Stopwatch _stopwatch;

        public async Task RunMutationTest(MuTestOptions options)
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _chalk = new Chalk();
            _options = options;

            _chalk.Default("\nPreparing Required Files...\n");

            var cppContexts = new CppDirectoryFactory
            {
                NumberOfMutantsExecutingInParallel = _options.ConcurrentTestRunners
            }.PrepareTestDirectories(
                options.TestClass,
                options.SourceClass,
                options.TestProject,
                options.TestSolution);
        }
    }
}