using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Cpp.CLI.Options;

namespace MuTest.Cpp.CLI
{
    public static class CliOptions
    {
        private static readonly MuTestOptions DefaultOptions = new MuTestOptions();

        public static readonly CliOption<string> TestClass = new CliOption<string>
        {
            ArgumentName = "--test-class",
            ArgumentShortName = "-tc <testClass>",
            ArgumentDescription = @"Used for matching the test code file required to find and execute test methods for mutation analysis. Example: ""<path>\ExampleTestClass.cpp"""
        };

        public static readonly CliOption<string> TestProject = new CliOption<string>
        {
            ArgumentName = "--test-project",
            ArgumentShortName = "-tp <testProject>",
            ArgumentDescription = @"Used for matching the test project references when finding the test project to build for each mutant. Example: ""ExampleProject.vcxproj"""
        };

        public static readonly CliOption<string> TestSolution = new CliOption<string>
        {
            ArgumentName = "--test-solution",
            ArgumentShortName = "-ts <testSolution>",
            ArgumentDescription = @"Used for matching the test solution to build and generate test output for each mutant. Example: ""<path>\ExampleSolution.sln"""
        };

        public static readonly CliOption<string> SourceClass = new CliOption<string>
        {
            ArgumentName = "--src-class",
            ArgumentShortName = "-sc <sourceClass>",
            ArgumentDescription = @"Used for matching the source code file references when finding the source code file to mutate. Example: ""<relative-path>\ExampleClass.cpp"""
        };

        public static readonly CliOption<string> SourceHeader = new CliOption<string>
        {
            ArgumentName = "--src-header",
            ArgumentShortName = "-sh <header>",
            ArgumentDescription = @"Used for matching the source code header file references when finding the source code file to mutate. Example: ""ExampleClass.h, ExampleClass.hpp"""
        };

        public static readonly CliOption<string> OutputPath = new CliOption<string>
        {
            ArgumentName = "--output-path",
            ArgumentShortName = "-o <outputPath>",
            ArgumentDescription = @"Mutation Result Output Path Example:""<path>\output.html, <path>\output.json"""
        };

        public static readonly CliOption<int> Parallel = new CliOption<int>
        {
            ArgumentName = "--parallel",
            ArgumentShortName = "-p <integer>",
            ArgumentDescription = "Set number of parallel mutant execution",
            DefaultValue = DefaultOptions.ConcurrentTestRunners,
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<string> BuildConfiguration = new CliOption<string>
        {
            ArgumentName = "--configuration",
            ArgumentShortName = "-c <configuration>",
            ArgumentDescription = @"Use for matching msbuild build configurations For Example: Debug, Release.."
        };

        public static readonly CliOption<string> Platform = new CliOption<string>
        {
            ArgumentName = "--platform",
            ArgumentShortName = "-pl <platform>",
            ArgumentDescription = "Use for matching msbuild platform For Example: x64, x86, AnyCpu",
            ValueType = CommandOptionType.SingleValue
        };

        public static readonly CliOption<bool> EnableDiagnostics = new CliOption<bool>
        {
            ArgumentName = "--enable-diagnostics",
            ArgumentShortName = "-d",
            ArgumentDescription = "Enable Diagnostics to see Mutation executing error logs -- Default is disabled",
            DefaultValue = DefaultOptions.EnableDiagnostics,
            ValueType = CommandOptionType.NoValue
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

        public static readonly CliOption<bool> InIsolation = new CliOption<bool>
        {
            ArgumentName = "--in-isolation",
            ArgumentShortName = "-i",
            ArgumentDescription = "Runs mutants in isolated test projects and solutions instead of sharing one test project and solution. It might run slower",
            DefaultValue = DefaultOptions.InIsolation,
            ValueType = CommandOptionType.NoValue
        };

        public static readonly CliOption<bool> DisableBuildOptimization = new CliOption<bool>
        {
            ArgumentName = "--disable-build-optimization",
            ArgumentShortName = "-do",
            ArgumentDescription = "Disable project build optimization.",
            DefaultValue = DefaultOptions.DisableBuildOptimization,
            ValueType = CommandOptionType.NoValue
        };
    }
}