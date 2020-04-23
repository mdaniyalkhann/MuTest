using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
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
                FullName = "MuTest: MuTest mutator for C++",
                Description = "MuTest mutator for C++",
                ExtendedHelpText = "Welcome to MuTest for .Net! Run dotnet muTest to kick off a mutation test run"
            };

            var testSolution = CreateOption(app, CliOptions.TestSolution);
            var sourceClass = CreateOption(app, CliOptions.SourceClass);
            var sourceHeader = CreateOption(app, CliOptions.SourceHeader);

            var testProject = CreateOption(app, CliOptions.TestProject);
            var testClass = CreateOption(app, CliOptions.TestClass);

            var parallel = CreateOption(app, CliOptions.Parallel);
            var survivedThreshold = CreateOption(app, CliOptions.SurvivedThreshold);
            var killedThreshold = CreateOption(app, CliOptions.KilledThreshold);
            var inIsolation = CreateOption(app, CliOptions.InIsolation);
            var diagnostic = CreateOption(app, CliOptions.EnableDiagnostics);
            var outputPath = CreateOption(app, CliOptions.OutputPath);
            var configuration = CreateOption(app, CliOptions.BuildConfiguration);
            var platform = CreateOption(app, CliOptions.Platform);

            app.HelpOption("--help | -h | -?");

            app.OnExecute(async () =>
            {
                var options = new OptionsBuilder
                {
                    Diagnostics = diagnostic,
                    TestSolution = testSolution,
                    SourceClass = sourceClass,
                    SourceHeader = sourceHeader,
                    TestProject = testProject,
                    TestClass = testClass,
                    Parallel = parallel,
                    OutputPath = outputPath,
                    Configuration = configuration,
                    Platform = platform,
                    SurvivedThreshold = survivedThreshold,
                    KilledThreshold = killedThreshold,
                    InIsolation = inIsolation
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
            Console.WriteLine(@"
╔╦╗┬ ┬╔╦╗┌─┐┌─┐┌┬┐  ╔═╗┌─┐┌─┐
║║║│ │ ║ ├┤ └─┐ │   ║  ├─┘├─┘
╩ ╩└─┘ ╩ └─┘└─┘ ┴   ╚═╝┴  ┴  
");
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;

            Console.WriteLine($@"
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
