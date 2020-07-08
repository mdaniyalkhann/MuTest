using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Console.Options;
using static System.Console;

namespace MuTest.Console
{
    public class MuTestCli
    {
        private readonly IMuTestRunner _muTest;

        public int ExitCode { get; set; }

        public MuTestCli(IMuTestRunner muTest)
        {
            _muTest = muTest;
            ExitCode = 0;
        }

        public int Run(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "MuTest",
                FullName = "MuTest: MuTest mutator for .Net",
                Description = "MuTest mutator for .Net",
                ExtendedHelpText = "Welcome to MuTest for .Net! Run dotnet muTest to kick off a mutation test run"
            };

            var sourceProject = CreateOption(app, CliOptions.SourceProject);
            var sourceLib = CreateOption(app, CliOptions.SourceLib);
            var sourceClass = CreateOption(app, CliOptions.SourceClass);
            var multipleSourceClasses = CreateOption(app, CliOptions.MultipleSourceClasses);

            var testProject = CreateOption(app, CliOptions.TestProject);
            var testLib = CreateOption(app, CliOptions.TestLib);
            var testClass = CreateOption(app, CliOptions.TestClass);
            var multipleTestClasses = CreateOption(app, CliOptions.MultipleTestClasses);

            var includePartialClasses = CreateOption(app, CliOptions.IncludePartialClasses);
            var analyzeExternalCoveredClasses = CreateOption(app, CliOptions.AnalyzeExternalCoveredClasses);
            var includeNestedClasses = CreateOption(app, CliOptions.IncludeNestedClasses);
            var executeAllTests = CreateOption(app, CliOptions.ExecuteAllTests);
            var findTestTime = CreateOption(app, CliOptions.TestExecutionThreshold);
            var skipTestProjectBuild = CreateOption(app, CliOptions.SkipTestProjectBuild);
            var useClassFilter = CreateOption(app, CliOptions.UseClassFilter);

            var parallel = CreateOption(app, CliOptions.Parallel);
            var mutantsPerLine = CreateOption(app, CliOptions.MutantsPerLine);
            var survivedThreshold = CreateOption(app, CliOptions.SurvivedThreshold);
            var killedThreshold = CreateOption(app, CliOptions.KilledThreshold);
            var x64 = CreateOption(app, CliOptions.X64TargetPlatform);
            var releaseMode = CreateOption(app, CliOptions.BuildInReleaseMode);
            var diagnostic = CreateOption(app, CliOptions.EnableDiagnostics);
            var optimizeTestProject = CreateOption(app, CliOptions.OptimizeTestProject);
            var outputPath = CreateOption(app, CliOptions.OutputPath);
            var noCoverage = CreateOption(app, CliOptions.NoCoverage);
            var filterMutantRegEx = CreateOption(app, CliOptions.Regex);
            var specificMutants = CreateOption(app, CliOptions.Specific);
            var className = CreateOption(app, CliOptions.ClassName);
            var processWholeProject = CreateOption(app, CliOptions.ProcessWholeProject);

            app.HelpOption("--help | -h | -?");

            app.OnExecute(async () =>
            {
                var options = new OptionsBuilder
                {
                    SourceProject = sourceProject,
                    Diagnostics = diagnostic,
                    SourceLib = sourceLib,
                    SourceClass = sourceClass,
                    MultipleSourceClasses = multipleSourceClasses,
                    TestProject = testProject,
                    TestLib = testLib,
                    TestClass = testClass,
                    MultipleTestClasses = multipleTestClasses,
                    IncludePartialClasses = includePartialClasses,
                    IncludeNestedClasses = includeNestedClasses,
                    ExecuteAllTests = executeAllTests,
                    FindTestTime = findTestTime,
                    SkipTestProjectBuild = skipTestProjectBuild,
                    UseClassFilter = useClassFilter,
                    Parallel = parallel,
                    ReleaseMode = releaseMode,
                    X64 = x64,
                    OutputPath = outputPath,
                    NoCoverage = noCoverage,
                    RegEx = filterMutantRegEx,
                    Specific = specificMutants,
                    ClassName = className,
                    ProcessWholeProject = processWholeProject,
                    SurvivedThreshold = survivedThreshold,
                    KilledThreshold = killedThreshold,
                    OptimizeTestProject = optimizeTestProject,
                    AnalyzeExternalCoveredClasses = analyzeExternalCoveredClasses,
                    MutantsPerLine = mutantsPerLine
                }.Build();

                await RunMuTest(options);
                return ExitCode;
            });
            return app.Execute(args);
        }

        private async Task RunMuTest(MuTestOptions options)
        {
            PrintAsciiName();
            await _muTest.RunMutationTest(options);
        }

        private static void PrintAsciiName()
        {
            WriteLine(@"
_  _ _  _ ___ ____ ____ ___ 
|\/| |  |  |  |___ [__   |  
|  | |__|  |  |___ ___]  |                              
");
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;

            WriteLine($@"
Version {assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}
");
        }

        private static CommandOption CreateOption<T>(CommandLineApplication app, CliOption<T> option)
        {
            return app.Option($"{option.ArgumentName} | {option.ArgumentShortName}",
                option.ArgumentDescription,
                option.ValueType);
        }
    }
}
