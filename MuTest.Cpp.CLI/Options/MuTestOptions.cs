using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const int DefaultConcurrentTestRunners = 5;
        private const double DefaultThreshold = 1.0;

        [JsonProperty("source-class")]
        public string SourceClass { get; set; }

        [JsonProperty("source-header")]
        public string SourceHeader { get; set; }

        [JsonProperty("test-class")]
        public string TestClass { get; set; }

        [JsonProperty("test-solution")]
        public string TestSolution { get; set; }

        [JsonProperty("test-project")]
        public string TestProject { get; set; }

        [JsonProperty("configuration")]
        public string Configuration { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("in-isolation")]
        public bool InIsolation { get; set; }

        [JsonProperty("enable-diagnostics")]
        public bool EnableDiagnostics { get; set; }

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

        [JsonProperty("disable-build-optimization")]
        public bool DisableBuildOptimization { get; set; }

        [JsonIgnore]
        public string OutputPath { get; set; }

        [JsonProperty("specific-lines-range")]
        public string SpecificLines { get; set; }

        [JsonProperty("include-build-events")]
        public bool IncludeBuildEvents { get; set; }

        public void ValidateOptions()
        {
            ValidateRequiredParameters();
            ValidateTestProject();
            ValidateTestSolution();

            if (!InIsolation)
            {
                ValidateSourceHeader();
            }

            ConcurrentTestRunners = ValidateConcurrentTestRunners();
            SetOutputPath();
            VerifySpecificLines();
        }

        private void VerifySpecificLines()
        {
            if (!string.IsNullOrWhiteSpace(SpecificLines))
            {
                const char separator = ':';
                var range = SpecificLines.Split(separator);
                if (range.Length == 2)
                {
                    var minValid = uint.TryParse(range[0], out var minimum);
                    var maxValid = uint.TryParse(range[1], out var maximum);

                    if (!minValid || !maxValid || maximum < minimum)
                    {
                        throw new MuTestInputException(ErrorMessage, $"Invalid Specific Line Range {CliOptions.SpecificLineRange.ArgumentDescription}");
                    }
                }
            }
        }

        private void ValidateSourceHeader()
        {
            if (string.IsNullOrWhiteSpace(SourceHeader))
            {
                var sourceClass = new FileInfo(SourceClass);
                var sourceClassExtension = Path.GetExtension(sourceClass.Name);

                if (sourceClassExtension.Equals(".h", StringComparison.InvariantCultureIgnoreCase) ||
                    sourceClassExtension.Equals(".hpp", StringComparison.InvariantCultureIgnoreCase))
                {
                    SourceHeader = SourceClass;
                    return;
                }

                var headerFile = new FileInfo($"{sourceClass.DirectoryName}\\{Path.GetFileNameWithoutExtension(sourceClass.Name)}.h");
                var headerCppFile = new FileInfo($"{sourceClass.DirectoryName}\\{Path.GetFileNameWithoutExtension(sourceClass.Name)}.hpp");

                if (headerFile.Exists)
                {
                    SourceHeader = headerFile.FullName;
                    return;
                }

                if (headerCppFile.Exists)
                {
                    SourceHeader = headerCppFile.FullName;
                    return;
                }

                throw new MuTestInputException(ErrorMessage, $"Unable to find Source header file. Valid options are {CliOptions.SourceHeader.ArgumentShortName}");
            }
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
            if (string.IsNullOrWhiteSpace(TestSolution) || !File.Exists(TestSolution))
            {
                var testSolution = new FileInfo(TestProject).FindCppSolutionFile(TestProject);
                if (testSolution != null)
                {
                    TestSolution = testSolution.FullName;
                }
            }

            if (!string.IsNullOrWhiteSpace(TestSolution))
            {
                var testSolution = new FileInfo(TestSolution);
                var projects = testSolution.FullName.GetProjects().ToList();
                var project = projects.First(x => x.AbsolutePath != null &&
                                              Path.GetFileName(x.AbsolutePath).Equals(Path.GetFileName(TestProject), StringComparison.InvariantCultureIgnoreCase));

                var parentFolders = new List<string>();
                var parentGuid = project.ParentProjectGuid;

                while (parentGuid != null)
                {
                    var parentProject = projects.FirstOrDefault(x => x.ProjectGuid == parentGuid);
                    if (parentProject == null)
                    {
                        break;
                    }

                    parentFolders.Add(parentProject.ProjectName);
                    parentGuid = parentProject.ParentProjectGuid;
                }

                var target = string.Empty;
                for (var index = parentFolders.Count - 1; index >= 0 ; index--)
                {
                    target = string.Join("\\", target, parentFolders[index]);
                }

                target = $"{target.Trim('\\')}\\{project.ProjectName}".Trim('\\');
                Target = target;

                return;
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