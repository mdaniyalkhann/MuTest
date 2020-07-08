using System;
using System.Collections;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Console.Options;
using MuTest.Core.Exceptions;
using Newtonsoft.Json;

namespace MuTest.Console
{
    public class OptionsBuilder
    {
        public CommandOption SourceProject { get; set; }

        public CommandOption SourceLib { get; set; }

        public CommandOption SourceClass { get; set; }

        public CommandOption TestProject { get; set; }

        public CommandOption TestLib { get; set; }

        public CommandOption TestClass { get; set; }

        public CommandOption IncludePartialClasses { get; set; }

        public CommandOption IncludeNestedClasses { get; set; }

        public CommandOption ExecuteAllTests { get; set; }

        public CommandOption UseClassFilter { get; set; }

        public CommandOption Parallel { get; set; }

        public CommandOption X64 { get; set; }

        public CommandOption ReleaseMode { get; set; }

        public CommandOption Diagnostics { get; set; }

        public CommandOption OutputPath { get; set; }

        public CommandOption NoCoverage { get; set; }

        public CommandOption RegEx { get; set; }

        public CommandOption Specific { get; set; }

        public CommandOption MultipleSourceClasses { get; set; }

        public CommandOption MultipleTestClasses { get; set; }

        public CommandOption ProcessWholeProject { get; set; }

        public CommandOption ClassName { get; set; }

        public CommandOption SurvivedThreshold { get; set; }

        public CommandOption KilledThreshold { get; set; }

        public CommandOption AnalyzeExternalCoveredClasses { get; set; }

        public CommandOption SkipTestProjectBuild { get; set; }

        public CommandOption OptimizeTestProject { get; set; }

        public CommandOption FindTestTime { get; set; }

        public CommandOption MutantsPerLine { get; set; }

        public MuTestOptions Build()
        {
            var muTestOptions = new MuTestOptions
            {
                SourceProjectParameter = GetOption(SourceProject.Value(), CliOptions.SourceProject),
                SourceProjectLibraryParameter = GetOption(SourceLib.Value(), CliOptions.SourceLib),
                SourceClassParameter = GetOption(SourceClass.Value(), CliOptions.SourceClass),
                TestProjectParameter = GetOption(TestProject.Value(), CliOptions.TestProject),
                TestProjectLibraryParameter = GetOption(TestLib.Value(), CliOptions.TestLib),
                TestClassParameter = GetOption(TestClass.Value(), CliOptions.TestClass),
                ExecuteAllTests = GetOption(ExecuteAllTests.Value(), CliOptions.ExecuteAllTests),
                SkipTestProjectBuild = GetOption(SkipTestProjectBuild.Value(), CliOptions.SkipTestProjectBuild),
                IncludePartialClasses = GetOption(IncludePartialClasses.Value(), CliOptions.IncludePartialClasses),
                AnalyzeExternalCoveredClasses = GetOption(AnalyzeExternalCoveredClasses.Value(), CliOptions.AnalyzeExternalCoveredClasses),
                IncludeNestedClasses = GetOption(IncludeNestedClasses.Value(), CliOptions.IncludeNestedClasses),
                UseClassFilter = GetOption(UseClassFilter.Value(), CliOptions.UseClassFilter),
                X64TargetPlatform = GetOption(X64.Value(), CliOptions.X64TargetPlatform),
                BuildInReleaseModeParameter = GetOption(ReleaseMode.Value(), CliOptions.BuildInReleaseMode),
                EnableDiagnostics = GetOption(Diagnostics.Value(), CliOptions.EnableDiagnostics),
                OptimizeTestProject = GetOption(OptimizeTestProject.Value(), CliOptions.OptimizeTestProject),
                ConcurrentTestRunners = GetOption(Parallel.Value(), CliOptions.Parallel),
                SurvivedThreshold = GetOption(SurvivedThreshold.Value(), CliOptions.SurvivedThreshold),
                KilledThreshold = GetOption(KilledThreshold.Value(), CliOptions.KilledThreshold),
                OutputPathParameter = GetOption(OutputPath.Value(), CliOptions.OutputPath),
                NoCoverage = GetOption(NoCoverage.Value(), CliOptions.NoCoverage),
                RegEx = GetOption(RegEx.Value(), CliOptions.Regex),
                Specific = GetOption(Specific.Value(), CliOptions.Specific),
                ClassName = GetOption(ClassName.Value(), CliOptions.ClassName),
                ProcessWholeProject = GetOption(ProcessWholeProject.Value(), CliOptions.ProcessWholeProject),
                TestExecutionThreshold = GetOption(FindTestTime.Value(), CliOptions.TestExecutionThreshold),
                MutantsPerLine = GetOption(MutantsPerLine.Value(), CliOptions.MutantsPerLine)
            };

            muTestOptions
                .MultipleSourceClasses
                .AddRange(GetOption(MultipleSourceClasses.Value(), CliOptions.MultipleSourceClasses).Distinct());
            muTestOptions
                .MultipleTestClasses
                .AddRange(GetOption(MultipleTestClasses.Value(), CliOptions.MultipleTestClasses).Distinct());

            muTestOptions.ValidateOptions();
            return muTestOptions;
        }

        private static T GetOption<TV, T>(TV cliValue, CliOption<T> option)
        {
            return cliValue != null
                ? ConvertTo(cliValue, option)
                : option.DefaultValue;
        }

        private static T ConvertTo<TV, T>(TV optionValue, CliOption<T> option)
        {
            try
            {
                if (typeof(IEnumerable).IsAssignableFrom(typeof(T)) && typeof(T) != typeof(string))
                {
                    var list = JsonConvert.DeserializeObject<T>(optionValue as string);
                    return list;
                }

                if (typeof(T) == typeof(bool))
                {
                    if (optionValue.ToString() == "on")
                    {
                        return (T)Convert.ChangeType(true, typeof(T));
                    }
                }

                return (T)Convert.ChangeType(optionValue, typeof(T));
            }
            catch (Exception ex)
            {
                throw new MuTestInputException("A optionValue passed to an option was not valid.", $@"The option {option.ArgumentName} with optionValue {optionValue} is not valid.
Hint:
{ex.Message}");
            }

        }
    }
}