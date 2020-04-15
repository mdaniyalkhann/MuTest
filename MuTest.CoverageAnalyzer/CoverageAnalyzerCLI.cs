using System;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.CoverageAnalyzer.Options;

namespace MuTest.CoverageAnalyzer
{
    public class CoverageAnalyzerCli
    {
        private readonly ICoverageAnalyzerRunner _analyzer;

        public int ExitCode { get; set; }

        public CoverageAnalyzerCli(ICoverageAnalyzerRunner muTest)
        {
            _analyzer = muTest;
            ExitCode = 0;
        }

        public int Run(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "Coverage Analyzer",
                FullName = "Coverage Analyzer: Coverage Analyzer for .Net",
                Description = "Coverage Analyzer analyze uncovered classes and generate JSON report",
                ExtendedHelpText = "Welcome to Coverage Analyzer for .Net!"
            };

            var codePath = CreateOption(app, CliOptions.CodePath);
            var codeCoveragePath = CreateOption(app, CliOptions.CodeCoveragePath);
            var coberturaReportRootPath = CreateOption(app, CliOptions.CoberturaReportRootPath);
            var specificSolutions = CreateOption(app, CliOptions.SpecificSolutions);
            var excludeSolutions = CreateOption(app, CliOptions.ExcludeSolutions);
            var fakesContainerProject = CreateOption(app, CliOptions.FakesContainerProject);
            var fakesContainerAssemblies = CreateOption(app, CliOptions.FakesContainerAssemblies);
            var commonFakesProject = CreateOption(app, CliOptions.CommonFakesProjectFormat);
            var outputDirectory = CreateOption(app, CliOptions.OutputDirectory);
            var testProjectMapFormat = CreateOption(app, CliOptions.TestProjectMapFormat);
            var branch = CreateOption(app, CliOptions.Branch);
            var repository = CreateOption(app, CliOptions.Repository);
            var projectId = CreateOption(app, CliOptions.ProjectId);
            var uncoveredThreshold = CreateOption(app, CliOptions.UncoveredThreshold);
            var coveredPercentage = CreateOption(app, CliOptions.CoveredPercentage);
            var projectConfiguration = CreateOption(app, CliOptions.ProjectConfiguration);

            app.HelpOption("--help | -h | -?");

            app.OnExecute(() =>
            {
                var options = new OptionsBuilder
                {
                    CodePath = codePath,
                    CodeCoveragePath = codeCoveragePath,
                    CoberturaReportRootPath = coberturaReportRootPath,
                    SpecificSolutions = specificSolutions,
                    ExcludeSolutions = excludeSolutions,
                    FakesContainerProject = fakesContainerProject,
                    FakesContainerAssemblies = fakesContainerAssemblies,
                    CommonFakesProjectFormat = commonFakesProject,
                    OutputDirectory = outputDirectory,
                    TestProjectMapFormat = testProjectMapFormat,
                    Branch = branch,
                    Repository = repository,
                    ProjectId = projectId,
                    UncoveredThreshold = uncoveredThreshold,
                    CoveredPercentage = coveredPercentage,
                    ProjectConfiguration = projectConfiguration
                }.Build();

                RunMuTest(options);
                return ExitCode;
            });
            return app.Execute(args);
        }

        private void RunMuTest(CoverageAnalyzerOptions options)
        {
            PrintAsciiName();
            _analyzer.RunAnalyzer(options);
        }

        private static void PrintAsciiName()
        {
            Console.WriteLine(@"
 _____                                        ___              _                      __   _____ 
/  __ \                                      / _ \            | |                    /  | |  _  |
| /  \/ _____   _____ _ __ __ _  __ _  ___  / /_\ \_ __   __ _| |_   _ _______ _ __  `| | | |/' |
| |    / _ \ \ / / _ \ '__/ _` |/ _` |/ _ \ |  _  | '_ \ / _` | | | | |_  / _ \ '__|  | | |  /| |
| \__/\ (_) \ V /  __/ | | (_| | (_| |  __/ | | | | | | | (_| | | |_| |/ /  __/ |    _| |_\ |_/ /
 \____/\___/ \_/ \___|_|  \__,_|\__, |\___| \_| |_/_| |_|\__,_|_|\__, /___\___|_|    \___(_)___/ 
                                 __/ |                            __/ |                          
                                |___/                            |___/                           
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
