using System.Collections.Generic;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Console.Options;

namespace MuTest.Console
{
    public static class CliOptions
    {
        private static readonly MuTestOptions DefaultOptions = new MuTestOptions();

        public static readonly CliOption<string> SourceProject = new CliOption<string>
        {
            ArgumentName = "--src-code-project",
            ArgumentShortName = "-sp <sourceProject>",
            ArgumentDescription = @"Used for matching the source project references when finding the project to mutate. Example: ""<path>\ExampleProject.csproj"""
        };

        public static readonly CliOption<string> SourceLib = new CliOption<string>
        {
            ArgumentName = "--src-code-library",
            ArgumentShortName = "-sl <sourceLibrary>",
            ArgumentDescription = @"Used for matching the source project library references when finding the project to mutate. Example: ""<path>\ExampleProject.dll, <path>\ExampleProject.exe"""
        };

        public static readonly CliOption<string> SourceClass = new CliOption<string>
        {
            ArgumentName = "--src-class",
            ArgumentShortName = "-sc <sourceClass>",
            ArgumentDescription = @"Used for matching the source code file references when finding the source code file to mutate. Example: ""<relative-path>\ExampleClass.cs, ExampleClass.cs"""
        };

        public static readonly CliOption<IList<string>> MultipleSourceClasses = new CliOption<IList<string>>
        {
            ArgumentName = "--mul-src-class",
            ArgumentShortName = "-msc <sourceClasses>",
            ValueType = CommandOptionType.MultipleValue,
            DefaultValue = DefaultOptions.MultipleSourceClasses,
            ArgumentDescription = @"Used for matching multiple source code files references when finding the source code files to mutate. Example: ""['ExampleClassA.cs', 'FolderA/ExampleClassB.cs']"""
        };

        public static readonly CliOption<string> TestProject = new CliOption<string>
        {
            ArgumentName = "--test-code-project",
            ArgumentShortName = "-tp <testProject>",
            ArgumentDescription = @"Used for matching the test project references when finding the test project to execute tests. Example: ""<path>\ExampleProject.csproj"""
        };

        public static readonly CliOption<string> TestLib = new CliOption<string>
        {
            ArgumentName = "--test-code-library",
            ArgumentShortName = "-tl <testLibrary>",
            ArgumentDescription = @"Used for matching the test project library references when finding the test project to execute tests. Example: ""<path>\ExampleProject.dll"""
        };

        public static readonly CliOption<string> TestClass = new CliOption<string>
        {
            ArgumentName = "--test-class",
            ArgumentShortName = "-tc <testClass>",
            ArgumentDescription = @"Used for matching the test code file references when finding the test code file to find test methods. Example: ""<path>\ExampleTestClass.cs, ExampleTestClass.cs"""
        };

        public static readonly CliOption<IList<string>> MultipleTestClasses = new CliOption<IList<string>>
        {
            ArgumentName = "--mul-test-class",
            ArgumentShortName = "-mtc <testClasses>",
            ValueType = CommandOptionType.MultipleValue,
            DefaultValue = DefaultOptions.MultipleTestClasses,
            ArgumentDescription = @"Used for matching multiple test code files references when finding the test code files to find test methods. Example: ""['ExampleTestClassA.cs', 'FolderA/ExampleTestClassB.cs']"""
        };

        public static readonly CliOption<string> OutputPath = new CliOption<string>
        {
            ArgumentName = "--output-path",
            ArgumentShortName = "-o <outputPath>",
            ArgumentDescription = @"Mutation Result Output Path Example:""<path>\output.html, <path>\output.json"""
        };

        public static readonly CliOption<string> Regex = new CliOption<string>
        {
            ArgumentName = "--regex",
            ArgumentShortName = "-rg <regex>",
            ArgumentDescription = @"Filter unnecessary Mutants using comma separated regular expressions Example:""Log.*,WriteLog.*"""
        };

        public static readonly CliOption<string> Specific = new CliOption<string>
        {
            ArgumentName = "--specific",
            ArgumentShortName = "-s <regex>",
            ArgumentDescription = @"Execute specific Mutants using comma separated regular expressions Example:""AddParameter.*,ExecuteSomeThing.*"""
        };

        public static readonly CliOption<bool> IncludePartialClasses = new CliOption<bool>
        {
            ArgumentName = "--include-partial-classes",
            ArgumentShortName = "-par",
            ArgumentDescription = "Include Tests in partial classes",
            DefaultValue = DefaultOptions.IncludePartialClasses,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> AnalyzeExternalCoveredClasses = new CliOption<bool>
        {
            ArgumentName = "--analyze-external-covered-classes",
            ArgumentShortName = "-ec",
            ArgumentDescription = "Analyze external covered classes",
            DefaultValue = DefaultOptions.AnalyzeExternalCoveredClasses,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> IncludeNestedClasses = new CliOption<bool>
        {
            ArgumentName = "--include-nested-classes",
            ArgumentShortName = "-ne",
            ArgumentDescription = "Include nested classes",
            DefaultValue = DefaultOptions.IncludeNestedClasses,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<string> ClassName = new CliOption<string>
        {
            ArgumentName = "--class-name",
            ArgumentShortName = "-cn <class-name>",
            ArgumentDescription = @"Specific name or fully qualified class name Example:""NamespaceA.ClassA, ClassA"""
        };

        public static readonly CliOption<string> ProcessWholeProject = new CliOption<string>
        {
            ArgumentName = "--process-whole-project",
            ArgumentShortName = "-pwp <test-class-regex>",
            ArgumentDescription = "Process whole project using test class format regex Example .*Test.cs, Test.*.cs",
            DefaultValue = DefaultOptions.ProcessWholeProject,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<bool> X64TargetPlatform = new CliOption<bool>
        {
            ArgumentName = "--x64-target-platform",
            ArgumentShortName = "-x64",
            ArgumentDescription = "x64 Project Platform -- Default is x86",
            DefaultValue = DefaultOptions.X64TargetPlatform,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> BuildInReleaseMode = new CliOption<bool>
        {
            ArgumentName = "--release-mode",
            ArgumentShortName = "-r",
            ArgumentDescription = "Build in release mode -- Default is Debug",
            DefaultValue = DefaultOptions.BuildInReleaseModeParameter,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> EnableDiagnostics = new CliOption<bool>
        {
            ArgumentName = "--enable-diagnostics",
            ArgumentShortName = "-d",
            ArgumentDescription = "Enable Diagnostics to see Mutation executing error logs -- Default is disabled",
            DefaultValue = DefaultOptions.EnableDiagnostics,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> OptimizeTestProject = new CliOption<bool>
        {
            ArgumentName = "--optimize-test-project",
            ArgumentShortName = "-otp",
            ArgumentDescription = "Optimize test project by excluding unrelated test files to save initial load time -- Default is disabled",
            DefaultValue = DefaultOptions.OptimizeTestProject,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> NoCoverage = new CliOption<bool>
        {
            ArgumentName = "--no-coverage",
            ArgumentShortName = "-n",
            ArgumentDescription = "Disable data diagnostic adapter CodeCoverage in the test run -- Default is enabled",
            DefaultValue = DefaultOptions.NoCoverage,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> ExecuteAllTests = new CliOption<bool>
        {
            ArgumentName = "--execute-all-tests",
            ArgumentShortName = "-ex-all",
            ArgumentDescription = "Execute All Tests for each Mutant",
            DefaultValue = DefaultOptions.ExecuteAllTests,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<int> TestExecutionThreshold = new CliOption<int>
        {
            ArgumentName = "--test-execution-threshold",
            ArgumentShortName = "-tet <threshold>",
            ArgumentDescription = "Find Tests with test Execution Time > then given threshold",
            DefaultValue = DefaultOptions.TestExecutionThreshold,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<bool> SkipTestProjectBuild = new CliOption<bool>
        {
            ArgumentName = "--skip-test-build",
            ArgumentShortName = "-stb",
            ArgumentDescription = "Skip Test Project Build",
            DefaultValue = DefaultOptions.SkipTestProjectBuild,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> UseClassFilter = new CliOption<bool>
        {
            ArgumentName = "--use-class-filter",
            ArgumentShortName = "-cf",
            ArgumentDescription = "Use Class Test Case Filter to find tests to run",
            DefaultValue = DefaultOptions.UseClassFilter,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<int> Parallel = new CliOption<int>
        {
            ArgumentName = "--parallel",
            ArgumentShortName = "-p <integer>",
            ArgumentDescription = "Set number of parallel mutant execution",
            DefaultValue = DefaultOptions.ConcurrentTestRunners,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<int> MutantsPerLine = new CliOption<int>
        {
            ArgumentName = "--mutants-per-line",
            ArgumentShortName = "-mpl <integer>",
            ArgumentDescription = "Set number of mutants per line Default is -1 (Unlimited)",
            DefaultValue = DefaultOptions.MutantsPerLine,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<double> SurvivedThreshold = new CliOption<double>
        {
            ArgumentName = "--survived-threshold",
            ArgumentShortName = "-st <double>",
            ArgumentDescription = "Set threshold to stop mutation analysis if number of survived mutants cross specific threshold Example 0.3 means stop mutation if 30% mutants are survived -- Default is 1.0",
            DefaultValue = DefaultOptions.SurvivedThreshold,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<double> KilledThreshold = new CliOption<double>
        {
            ArgumentName = "--killed-threshold",
            ArgumentShortName = "-kt <double>",
            ArgumentDescription = "Set threshold to stop mutation analysis if number of killed mutants cross specific threshold Example 0.7 means stop mutation if 70% mutants are killed -- Default is 1.0",
            DefaultValue = DefaultOptions.KilledThreshold,
            ValueType = CommandOptionType.SingleValue
        };
    }
}