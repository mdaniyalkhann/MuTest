using System;
using System.Collections;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Core.Exceptions;
using MuTest.Cpp.CLI.Options;
using Newtonsoft.Json;

namespace MuTest.Cpp.CLI
{
    public class OptionsBuilder
    {
        public CommandOption TestSolution { get; set; }

        public CommandOption SourceClass { get; set; }

        public CommandOption TestProject { get; set; }

        public CommandOption TestClass { get; set; }

        public CommandOption Parallel { get; set; }

        public CommandOption Diagnostics { get; set; }

        public CommandOption OutputPath { get; set; }

        public CommandOption Platform { get; set; }

        public CommandOption Configuration { get; set; }

        public CommandOption SurvivedThreshold { get; set; }

        public CommandOption KilledThreshold { get; set; }

        public CommandOption InIsolation { get; set; }

        public CommandOption SourceHeader { get; set; }

        public CommandOption DisableBuildOptimization { get; set; }

        public CommandOption SpecificLines { get; set; }

        public CommandOption IncludeBuildEvents { get; set; }

        public CommandOption MutantsPerLine { get; set; }


        public MuTestOptions Build()
        {
            var muTestOptions = new MuTestOptions
            {
                TestSolution = GetOption(TestSolution.Value(), CliOptions.TestSolution),
                SourceClass = GetOption(SourceClass.Value(), CliOptions.SourceClass),
                SourceHeader = GetOption(SourceHeader.Value(), CliOptions.SourceHeader),
                TestProject = GetOption(TestProject.Value(), CliOptions.TestProject),
                TestClass = GetOption(TestClass.Value(), CliOptions.TestClass),
                EnableDiagnostics = GetOption(Diagnostics.Value(), CliOptions.EnableDiagnostics),
                ConcurrentTestRunners = GetOption(Parallel.Value(), CliOptions.Parallel),
                MutantsPerLine = GetOption(MutantsPerLine.Value(), CliOptions.MutantsPerLine),
                SurvivedThreshold = GetOption(SurvivedThreshold.Value(), CliOptions.SurvivedThreshold),
                KilledThreshold = GetOption(KilledThreshold.Value(), CliOptions.KilledThreshold),
                InIsolation = GetOption(InIsolation.Value(), CliOptions.InIsolation),
                DisableBuildOptimization = GetOption(DisableBuildOptimization.Value(), CliOptions.DisableBuildOptimization),
                IncludeBuildEvents = GetOption(IncludeBuildEvents.Value(), CliOptions.IncludeBuildEvents),
                OutputPath = GetOption(OutputPath.Value(), CliOptions.OutputPath),
                Configuration = GetOption(Configuration.Value(), CliOptions.BuildConfiguration),
                Platform = GetOption(Platform.Value(), CliOptions.Platform),
                SpecificLines = GetOption(SpecificLines.Value(), CliOptions.SpecificLineRange)
            };

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
                throw new MuTestInputException("\nA optionValue passed to an option was not valid.", $@"The option {option.ArgumentName} with optionValue {optionValue} is not valid.
Hint:
{ex.Message}\n");
            }

        }
    }
}