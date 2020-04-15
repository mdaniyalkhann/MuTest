using System;
using System.IO;
using MuTest.Core.Common;
using MuTest.Core.Exceptions;
using MuTest.Cpp.CLI.Utility;
using Newtonsoft.Json;

namespace MuTest.Cpp.CLI.Options
{
    public class MuTestOptions
    {
        private const string ErrorMessage = "The value for one of your settings is not correct. Try correcting or removing them.";
        private const string JsonExtension = ".json";
        private const string HtmlExtension = ".html";
        private const int DefaultConcurrentTestRunners = 4;
        private const double DefaultThreshold = 1.0;

        [JsonProperty("test-solution")]
        public string TestSolution { get; set; }

        [JsonProperty("source-class")]
        public string SourceClass { get; set; }

        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("test-project")]
        public string TestProject { get; set; }

        [JsonProperty("test-class")]
        public string TestClass { get; set; }

        [JsonProperty("concurrent-test-runners")]
        public int ConcurrentTestRunners { get; set; } = DefaultConcurrentTestRunners;

        [JsonProperty("survived-threshold")]
        public double SurvivedThreshold { get; set; } = DefaultThreshold;

        [JsonProperty("killed-threshold")]
        public double KilledThreshold { get; set; } = DefaultThreshold;

        [JsonProperty("html-output")]
        public string HtmlOutputPath { get; private set; }

        [JsonProperty("json-output")]
        public string JsonOutputPath { get; private set; }

        [JsonIgnore]
        public string OutputPath { get; set; }

        [JsonProperty("enable-diagnostics")]
        public bool EnableDiagnostics { get; set; }

        public void ValidateOptions()
        {
            ValidateRequiredParameters();
            ValidateTestProject();
            ValidateTestSolution();
            ConcurrentTestRunners = ValidateConcurrentTestRunners();
            SetOutputPath();
        }

        private void ValidateTestProject()
        {
            if (string.IsNullOrWhiteSpace(TestProject))
            {
                var testProject = new FileInfo(TestClass).FindCppProjectFile();
                if (testProject != null)
                {
                    TestProject = testProject.FullName;
                }
            }

            if (!File.Exists(TestProject))
            {
                throw new MuTestInputException(ErrorMessage, $"Unable to find Test Project. Valid options are {CliOptions.TestProject.ArgumentShortName}");
            }
        }

        private void ValidateTestSolution()
        {
            if (string.IsNullOrWhiteSpace(TestSolution))
            {
                var testSolution = new FileInfo(TestProject).FindCppSolutionFile();
                if (testSolution != null)
                {
                    TestSolution = testSolution.FullName;
                }
            }

            if (!File.Exists(TestSolution))
            {
                throw new MuTestInputException(ErrorMessage, $"Unable to find Test Project Solution. Valid options are {CliOptions.TestSolution.ArgumentShortName}");
            }
        }

        private void SetOutputPath()
        {
            if (string.IsNullOrWhiteSpace(OutputPath))
            {
                var currentDateTime = DateTime.Now;
                OutputPath = $@"Results\{currentDateTime:yyyyMdhhmmss}\Mutation_Report_{Constants.SourceClassPlaceholder}";
                HtmlOutputPath = $"{OutputPath}.html";
                JsonOutputPath = $"{OutputPath}.json";

                return;
            }

            if (OutputPath.EndsWith(JsonExtension))
            {
                JsonOutputPath = OutputPath;
                HtmlOutputPath = OutputPath.Replace(JsonExtension, HtmlExtension);
                return;
            }

            if (OutputPath.EndsWith(HtmlExtension))
            {
                HtmlOutputPath = OutputPath;
                HtmlOutputPath = OutputPath.Replace(HtmlExtension, JsonExtension);
                return;
            }

            throw new MuTestInputException("Output Path is invalid", CliOptions.OutputPath.ArgumentDescription);
        }

        private void ValidateRequiredParameters()
        {
            if (string.IsNullOrWhiteSpace(TestClass) ||
                !File.Exists(TestClass))
            {
                throw new MuTestInputException(ErrorMessage, $"The Test Class file is required. Valid Options are {CliOptions.TestClass.ArgumentShortName}");
            }

            if (string.IsNullOrWhiteSpace(SourceClass) ||
                !File.Exists(SourceClass))
            {
                throw new MuTestInputException(ErrorMessage, $"The Source Class file is required. Valid Options are {CliOptions.SourceClass.ArgumentShortName}");
            }
        }

        private int ValidateConcurrentTestRunners()
        {
            if (ConcurrentTestRunners < 1)
            {
                ConcurrentTestRunners = DefaultConcurrentTestRunners;
            }

            var logicalProcessorCount = Environment.ProcessorCount;
            var usableProcessorCount = Math.Max(logicalProcessorCount, 1);

            if (ConcurrentTestRunners <= logicalProcessorCount)
            {
                usableProcessorCount = ConcurrentTestRunners;
            }

            return usableProcessorCount;
        }
    }
}