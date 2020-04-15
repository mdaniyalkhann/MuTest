using System;
using System.Collections;
using System.Linq;
using Microsoft.Extensions.CommandLineUtils;
using MuTest.CLI.Core;
using MuTest.Core.Exceptions;
using MuTest.CoverageAnalyzer.Options;
using Newtonsoft.Json;

namespace MuTest.CoverageAnalyzer
{
    public class OptionsBuilder
    {
        public CommandOption CodePath { get; set; }

        public CommandOption CodeCoveragePath { get; set; }

        public CommandOption CoberturaReportRootPath { get; set; }

        public CommandOption FakesContainerProject { get; set; }

        public CommandOption FakesContainerAssemblies { get; set; }

        public CommandOption CommonFakesProjectFormat { get; set; }

        public CommandOption TestProjectMapFormat { get; set; }

        public CommandOption OutputDirectory { get; set; }

        public CommandOption SpecificSolutions { get; set; }

        public CommandOption ExcludeSolutions { get; set; }

        public CommandOption Branch { get; set; }

        public CommandOption Repository { get; set; }

        public CommandOption ProjectId { get; set; }

        public CommandOption UncoveredThreshold { get; set; }

        public CommandOption CoveredPercentage { get; set; }

        public CommandOption ProjectConfiguration { get; set; }

        public CoverageAnalyzerOptions Build()
        {
            var options = new CoverageAnalyzerOptions
            {
                CodePathParameter = GetOption(CodePath.Value(), CliOptions.CodePath),
                CodeCoverageParameter = GetOption(CodeCoveragePath.Value(), CliOptions.CodeCoveragePath),
                CoberturaReportRootFolderPath = GetOption(CoberturaReportRootPath.Value(), CliOptions.CoberturaReportRootPath),
                FakesContainerProject = GetOption(FakesContainerProject.Value(), CliOptions.FakesContainerProject),
                FakesContainerAssemblies = GetOption(FakesContainerAssemblies.Value(), CliOptions.FakesContainerAssemblies),
                CommonFakesProjectFormat = GetOption(CommonFakesProjectFormat.Value(), CliOptions.CommonFakesProjectFormat),
                TestProjectMapFormat = GetOption(TestProjectMapFormat.Value(), CliOptions.TestProjectMapFormat),
                OutputDirectoryParameter = GetOption(OutputDirectory.Value(), CliOptions.OutputDirectory),
                BranchParameter = GetOption(Branch.Value(), CliOptions.Branch),
                RepositoryParameter = GetOption(Repository.Value(), CliOptions.Repository),
                ProjectIdParameter = GetOption(ProjectId.Value(), CliOptions.ProjectId),
                UncoveredThresholdParameter = GetOption(UncoveredThreshold.Value(), CliOptions.UncoveredThreshold),
                CoveredPercentageParameter = GetOption(CoveredPercentage.Value(), CliOptions.CoveredPercentage),
                ProjectConfigurationParameter = GetOption(ProjectConfiguration.Value(), CliOptions.ProjectConfiguration)
            };

            options
                .SpecificSolutions
                .AddRange(GetOption(SpecificSolutions.Value(), CliOptions.SpecificSolutions).Distinct());
            options
                .ExcludeSolutions
                .AddRange(GetOption(ExcludeSolutions.Value(), CliOptions.ExcludeSolutions).Distinct());

            options.ValidateOptions();
            return options;
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
                throw new MuTestInputException("A option value passed to an option was not valid.", $@"The option {option.ArgumentName} with optionValue {optionValue} is not valid.
Hint:
{ex.Message}");
            }

        }
    }
}