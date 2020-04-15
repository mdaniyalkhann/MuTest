using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.CoverageAnalyzer.Options;

namespace MuTest.CoverageAnalyzer
{
    public static class CliOptions
    {
        private static readonly CoverageAnalyzerOptions DefaultOptions = new CoverageAnalyzerOptions();

        public static readonly CliOption<string> CodePath = new CliOption<string>
        {
            ArgumentName = "--code-directory",
            ArgumentShortName = "-cd <codeDirectory>",
            ArgumentDescription = @"Used for matching the repository root path where code exists. Example: ""C:\RepoA\"""
        };

        public static readonly CliOption<string> CodeCoveragePath = new CliOption<string>
        {
            ArgumentName = "--code-coverage",
            ArgumentShortName = "-cc <coveragePath>",
            ArgumentDescription = @"Used for matching the repository visual studio code coverage file or cobertura coverage file. Example: ""<path>\file.coverage or <path>\file.coveragexml"""
        };

        public static readonly CliOption<string> CoberturaReportRootPath = new CliOption<string>()
        {
            ArgumentName = "--cobertura-report-root",
            ArgumentShortName = "-crr <coveragePath>",
            ArgumentDescription = @"Used for matching the root path of the files contained in the Cobertura report",
            DefaultValue = null
        };

        public static readonly CliOption<IList<string>> SpecificSolutions = new CliOption<IList<string>>
        {
            ArgumentName = "--specific-solutions",
            ArgumentShortName = "-ss <specificSolutions>",
            ValueType = CommandOptionType.MultipleValue,
            DefaultValue = DefaultOptions.SpecificSolutions,
            ArgumentDescription = @"Used for matching specific solutions to analyze. Example: ""['SolutionA.sln', 'FolderA/SolutionB.sln']"""
        };

        public static readonly CliOption<IList<string>> ExcludeSolutions = new CliOption<IList<string>>
        {
            ArgumentName = "--exclude-solutions",
            ArgumentShortName = "-exs <excludeSolutions>",
            ValueType = CommandOptionType.MultipleValue,
            DefaultValue = DefaultOptions.ExcludeSolutions,
            ArgumentDescription = @"Used for excluding specific solutions to analyze. Example: ""['SolutionA.sln', 'FolderA/SolutionB.sln']"""
        };

        public static readonly CliOption<string> FakesContainerProject = new CliOption<string>
        {
            ArgumentName = "--fakes-container-project",
            ArgumentShortName = "-fkp <fakesContainerProject>",
            ArgumentDescription = @"Used for matching the fake container project to store system and third party fakes. Example: ""<path>\FakeContainer.csproj"""
        };

        public static readonly CliOption<string> FakesContainerAssemblies = new CliOption<string>
        {
            ArgumentName = "--fakes-container-assemblies",
            ArgumentShortName = "-fka <fakesContainerAssemblies>",
            ArgumentDescription = @"Used for matching fakes container assemblies path. Example: ""<path>\FakeAssemblies"""
        };

        public static readonly CliOption<string> ProjectId = new CliOption<string>
        {
            ArgumentName = "--project-id",
            ArgumentShortName = "-p <projectId>",
            ArgumentDescription = @"Used for matching Jira project id. Example: ""1234"""
        };

        public static readonly CliOption<string> CommonFakesProjectFormat = new CliOption<string>
        {
            ArgumentName = "--common-fakes-project-format",
            ArgumentShortName = "-cfp <commonFakesProject>",
            DefaultValue = DefaultOptions.CommonFakesProjectFormat,
            ArgumentDescription = @"Used for matching the common fakes project in solution. Default: ""CommonFakes.Tests"" other Examples: ""CommonFakes.UnitTests or {solution-name}CommonFakes.Tests"""
        };

        public static readonly CliOption<string> OutputDirectory = new CliOption<string>
        {
            ArgumentName = "--output-directory",
            ArgumentShortName = "-o <outputDirectory>",
            DefaultValue = DefaultOptions.OutputDirectoryParameter,
            ArgumentDescription = @"Result Output Path Example:""<path>"""
        };

        public static readonly CliOption<string> TestProjectMapFormat = new CliOption<string>
        {
            ArgumentName = "--test-project-format",
            ArgumentShortName = "-tpf <testProjectFormat>",
            DefaultValue = DefaultOptions.TestProjectMapFormat,
            ArgumentDescription = @"Used for matching test project with respect to source project using format. Default: ""{project-name}.Tests"""
        };

        public static readonly CliOption<string> Branch = new CliOption<string>
        {
            ArgumentName = "--branch",
            ArgumentShortName = "-b <branch>",
            DefaultValue = DefaultOptions.BranchParameter,
            ArgumentDescription = $@"Used for matching repository branch name. Default: ""{DefaultOptions.BranchParameter}"""
        };

        public static readonly CliOption<string> ProjectConfiguration = new CliOption<string>
        {
            ArgumentName = "--project-configuration",
            ArgumentShortName = "-pc <projectConfiguration>",
            DefaultValue = DefaultOptions.ProjectConfigurationParameter,
            ArgumentDescription = $@"Used for setting the project configuration. Default: $""{DefaultOptions.ProjectConfigurationParameter}"""
        };

        public static readonly CliOption<string> Repository = new CliOption<string>
        {
            ArgumentName = "--repository-name",
            ArgumentShortName = "-rn <repositoryName>",
            DefaultValue = DefaultOptions.RepositoryParameter,
            ArgumentDescription = @"Used for matching repository name or url."
        };

        public static readonly CliOption<int> UncoveredThreshold = new CliOption<int>
        {
            ArgumentName = "--uncovered-threshold",
            ArgumentShortName = "-ut <uncoveredThreshold>",
            DefaultValue = DefaultOptions.UncoveredThresholdParameter,
            ArgumentDescription = $@"Using for defining uncovered line threshold to consider class. Default: {DefaultOptions.UncoveredThresholdParameter}"
        };

        public static readonly CliOption<int> CoveredPercentage = new CliOption<int>
        {
            ArgumentName = "--covered-percentage",
            ArgumentShortName = "-cp <uncoveredThreshold>",
            DefaultValue = DefaultOptions.CoveredPercentageParameter,
            ArgumentDescription = $@"Using for defining percentage to consider class if line coverage > --uncovered-threshold but partially covered Default: {DefaultOptions.CodeCoverageParameter}"
        };
    }
}