using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Exceptions;
using MuTest.Core.Model;
using MuTest.Core.Model.ClassDeclarations;
using MuTest.Core.Utility;
using Newtonsoft.Json;
using static MuTest.Core.Common.Constants;

namespace MuTest.Console.Options
{
    public class MuTestOptions
    {
        private const string ErrorMessage = "The value for one of your settings is not correct. Try correcting or removing them.";
        private const string JsonExtension = ".json";
        private const string HtmlExtension = ".html";
        private const int DefaultConcurrentTestRunners = 4;
        private const int DefaultMutantsPerLine = 1;
        private const double DefaultThreshold = 1.0;

        [JsonProperty("source-project")]
        public string SourceProjectParameter { get; set; }

        [JsonProperty("source-project-library")]
        public string SourceProjectLibraryParameter { get; set; }

        [JsonProperty("source-class")]
        public string SourceClassParameter { get; set; }

        [JsonProperty("class-name")]
        public string ClassName { get; set; }

        [JsonProperty("test-project")]
        public string TestProjectParameter { get; set; }

        [JsonProperty("test-project-library")]
        public string TestProjectLibraryParameter { get; set; }

        [JsonProperty("test-class")]
        public string TestClassParameter { get; set; }

        [JsonProperty("mutant-filter")]
        public string RegEx { get; set; } = string.Empty;

        [JsonProperty("concurrent-test-runners")]
        public int ConcurrentTestRunners { get; set; } = DefaultConcurrentTestRunners;

        [JsonProperty("mutants-per-line")]
        public int MutantsPerLine { get; set; } = DefaultMutantsPerLine;

        [JsonProperty("survived-threshold")]
        public double SurvivedThreshold { get; set; } = DefaultThreshold;

        [JsonProperty("killed-threshold")]
        public double KilledThreshold { get; set; } = DefaultThreshold;

        [JsonProperty("html-output")]
        public string HtmlOutputPath { get; private set; }

        [JsonProperty("json-output")]
        public string JsonOutputPath { get; private set; }

        [JsonIgnore]
        public string OutputPathParameter { get; set; }

        [JsonProperty("include-partial-classes")]
        public bool IncludePartialClasses { get; set; }

        [JsonProperty("analyze-external-covered-classes")]
        public bool AnalyzeExternalCoveredClasses { get; set; }

        [JsonProperty("include-nested-classes")]
        public bool IncludeNestedClasses { get; set; }

        [JsonProperty("x64")]
        public bool X64TargetPlatform { get; set; }

        [JsonProperty("execute-all-tests")]
        public bool ExecuteAllTests { get; set; }

        [JsonProperty("test-execution-threshold")]
        public int TestExecutionThreshold { get; set; } = -1;

        [JsonProperty("skip-test-project-build")]
        public bool SkipTestProjectBuild { get; set; }

        [JsonProperty("use-class-filter")]
        public bool UseClassFilter { get; set; }

        [JsonProperty("process-whole-project")]
        public string ProcessWholeProject { get; set; }

        [JsonProperty("build-in-release-mode")]
        public bool BuildInReleaseModeParameter { get; set; }

        [JsonProperty("enable-diagnostics")]
        public bool EnableDiagnostics { get; set; }

        [JsonProperty("no-coverage")]
        public bool NoCoverage { get; set; }

        [JsonProperty("specific-mutants-regex")]
        public string Specific { get; set; }

        [JsonIgnore]
        public IList<TargetClass> MultipleTargetClasses { get; } = new List<TargetClass>();

        [JsonIgnore]
        public List<string> MultipleTestClasses { get; } = new List<string>();

        [JsonIgnore]
        public List<string> MultipleSourceClasses { get; } = new List<string>();

        [JsonProperty("optimize-test-project")]
        public bool OptimizeTestProject { get; set; }

        public void ValidateOptions()
        {
            ValidateRequiredParameters();
            ValidateSourceLib();
            ValidateTestLib();

            if (string.IsNullOrWhiteSpace(ProcessWholeProject))
            {
                ValidateSourceClass();
                ValidateSourceClasses();
                ValidateTestClass();
                ValidateTestClasses();
                PrepareTargetClasses();
            }
            else
            {
                ValidateWholeProject();
            }

            ConcurrentTestRunners = ValidateConcurrentTestRunners();
            SetOutputPath();
        }

        private void PrepareTargetClasses()
        {
            MultipleTargetClasses.Clear();
            for (var index = 0; index < MultipleSourceClasses.Count; index++)
            {
                string sourceClass = MultipleSourceClasses[index];
                string testClass = MultipleTestClasses[index];

                var classes = sourceClass.GetCodeFileContent().RootNode().ClassNodes(ClassName);
                foreach (var claz in classes)
                {
                    MultipleTargetClasses.Add(new TargetClass
                    {
                        ClassName = claz.FullName(),
                        ClassPath = sourceClass,
                        TestClassPath = testClass
                    });
                }
            }
        }

        private void ValidateWholeProject()
        {
            var sourceClasses = new FileInfo(SourceProjectParameter).GetProjectFiles();
            var testClasses = new FileInfo(TestProjectParameter).GetProjectFiles();
            MultipleTargetClasses.Clear();

            foreach (string key in sourceClasses.Keys)
            {
                var extension = Path.GetExtension(key);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    var classPath = sourceClasses[key];
                    var fileName = classPath.Replace(extension, string.Empty);
                    var classes = classPath.GetCodeFileContent().RootNode().DescendantNodes<ClassDeclarationSyntax>().ToList();

                    foreach (var claz in classes)
                    {
                        var testKey = testClasses.FindKey(claz.ClassName() + ProcessWholeProject);
                        testKey = testKey ?? testClasses.FindKey(fileName + ProcessWholeProject);

                        if (!string.IsNullOrWhiteSpace(testKey))
                        {
                            MultipleTargetClasses.Add(new TargetClass
                            {
                                ClassName = claz.FullName(),
                                ClassPath = classPath,
                                TestClassPath = testClasses[testKey]
                            });
                        }
                    }
                }
            }

            if (!MultipleTargetClasses.Any())
            {
                throw new MuTestInputException($"No any class found with matching Test format {ProcessWholeProject}", CliOptions.ProcessWholeProject.ArgumentDescription);
            }
        }

        private void SetOutputPath()
        {
            if (string.IsNullOrWhiteSpace(OutputPathParameter))
            {
                var currentDateTime = DateTime.Now;
                OutputPathParameter = $@"Results\{currentDateTime:yyyyMdhhmmss}\Mutation_Report_{SourceClassPlaceholder}";
                HtmlOutputPath = $"{OutputPathParameter}.html";
                JsonOutputPath = $"{OutputPathParameter}.json";

                return;
            }

            if (OutputPathParameter.EndsWith(JsonExtension))
            {
                JsonOutputPath = OutputPathParameter;
                HtmlOutputPath = OutputPathParameter.Replace(JsonExtension, HtmlExtension);
                return;
            }

            if (OutputPathParameter.EndsWith(HtmlExtension))
            {
                HtmlOutputPath = OutputPathParameter;
                HtmlOutputPath = OutputPathParameter.Replace(HtmlExtension, JsonExtension);
                return;
            }

            throw new MuTestInputException("Output Path is invalid", CliOptions.OutputPath.ArgumentDescription);
        }

        private void ValidateSourceClass()
        {
            if (File.Exists(SourceClassParameter) ||
                MultipleSourceClasses.Any())
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(SourceClassParameter))
            {
                var classes = new FileInfo(SourceProjectParameter).GetProjectFiles();
                var file = classes.FindKey(SourceClassParameter);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    var path = new FileInfo(classes[file]);
                    if (path.Exists)
                    {
                        SourceClassParameter = path.FullName;
                        return;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(SourceClassParameter))
            {
                var classes = SourceProjectParameter.GetCSharpClassDeclarationsFromProject();
                if (classes.Any())
                {
                    var sourceClassDetails = classes.SelectMany(cu => cu.CompilationUnitSyntax.DescendantNodes<ClassDeclarationSyntax>(),
                        (cu, claz) => new SourceClassDetail
                        {
                            Claz = new ClassDeclaration(claz),
                            FullName = $"{cu.CompilationUnitSyntax.NameSpace()}.{claz.Identifier.Text}",
                            FilePath = cu.FileName
                        })
                        .OrderBy(x => x.Claz.Syntax.Ancestors<ClassDeclarationSyntax>().Count).ToList();

                    SourceClassParameter = sourceClassDetails.FirstOrDefault(x => x.FullName.Equals(ClassName, StringComparison.InvariantCultureIgnoreCase))?.FilePath;

                    if (string.IsNullOrWhiteSpace(SourceClassParameter))
                    {
                        SourceClassParameter = sourceClassDetails.FirstOrDefault(x => x.Claz.Syntax.ClassName().Equals(ClassName, StringComparison.CurrentCultureIgnoreCase))?.FilePath;

                        if (!string.IsNullOrWhiteSpace(SourceClassParameter))
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

            }

            throw new MuTestInputException(ErrorMessage, $"Source class file (.cs) or class with class name {ClassName} is not exist. {CliOptions.SourceClass.ArgumentDescription}");
        }

        private void ValidateSourceClasses()
        {
            if (!string.IsNullOrWhiteSpace(SourceClassParameter) &&
                !MultipleSourceClasses.Any())
            {
                MultipleSourceClasses.Add(SourceClassParameter);
                return;
            }

            var isAllClassesFound = true;
            var sourceClasses = new List<string>();
            foreach (var sourceClass in MultipleSourceClasses)
            {
                var classes = new FileInfo(SourceProjectParameter).GetProjectFiles();
                var file = classes.FindKey(sourceClass);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    var path = new FileInfo(classes[file]);
                    if (path.Exists)
                    {
                        sourceClasses.Add(path.FullName);
                    }
                    else
                    {
                        isAllClassesFound = false;
                        break;
                    }
                }
                else
                {
                    isAllClassesFound = false;
                    break;
                }
            }

            if (isAllClassesFound)
            {
                MultipleSourceClasses.Clear();
                MultipleSourceClasses.AddRange(sourceClasses);
                return;
            }

            throw new MuTestInputException(ErrorMessage, $"One or more Source class file(s) (.cs) are not exist. {CliOptions.MultipleSourceClasses.ArgumentDescription}");
        }

        private void ValidateTestClass()
        {
            if (File.Exists(TestClassParameter) ||
                MultipleTestClasses.Any())
            {
                return;
            }

            var classes = new FileInfo(TestProjectParameter).GetProjectFiles();
            var file = classes.FindKey(TestClassParameter);
            if (!string.IsNullOrWhiteSpace(file))
            {
                var path = new FileInfo(classes[file]);
                if (path.Exists)
                {
                    TestClassParameter = path.FullName;
                    return;
                }
            }

            throw new MuTestInputException(ErrorMessage, $"Test class file (.cs) is not exist. {CliOptions.TestClass.ArgumentDescription}");
        }

        private void ValidateTestClasses()
        {
            if (!string.IsNullOrWhiteSpace(TestClassParameter) &&
                !MultipleTestClasses.Any())
            {
                MultipleSourceClasses
                    .ForEach(x => MultipleTestClasses.Add(TestClassParameter));
                return;
            }

            var isAllClassesFound = true;
            var testClasses = new List<string>();
            foreach (var testClass in MultipleTestClasses)
            {
                var classes = new FileInfo(TestProjectParameter).GetProjectFiles();
                var file = classes.FindKey(testClass);
                if (!string.IsNullOrWhiteSpace(file))
                {
                    var path = new FileInfo(classes[file]);
                    if (path.Exists)
                    {
                        testClasses.Add(path.FullName);
                    }
                    else
                    {
                        isAllClassesFound = false;
                        break;
                    }
                }
                else
                {
                    isAllClassesFound = false;
                    break;
                }
            }

            if (isAllClassesFound)
            {
                MultipleTestClasses.Clear();
                MultipleTestClasses.AddRange(testClasses);
                return;
            }

            throw new MuTestInputException(ErrorMessage, $"One or more Test class file(s) (.cs) are not exist. {CliOptions.MultipleTestClasses.ArgumentDescription}");
        }

        private void ValidateSourceLib()
        {
            if (File.Exists(SourceProjectLibraryParameter))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SourceProjectLibraryParameter))
            {
                var projectFile = new FileInfo(SourceProjectParameter).FindLibraryPath();
                if (projectFile != null)
                {
                    SourceProjectLibraryParameter = projectFile.FullName;
                    return;
                }
            }

            if (SourceProjectLibraryParameter != null)
            {
                var file = Path.GetDirectoryName(SourceProjectParameter).FindFile(SourceProjectLibraryParameter);
                if (file != null)
                {
                    SourceProjectLibraryParameter = file.FullName;
                    return;
                }
            }

            throw new MuTestInputException(ErrorMessage, $"Source Project Library (.dll/.exe) is not exist. {CliOptions.SourceLib.ArgumentDescription}");
        }

        private void ValidateTestLib()
        {
            if (File.Exists(TestProjectLibraryParameter))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(TestProjectLibraryParameter))
            {
                var projectFile = new FileInfo(TestProjectParameter).FindLibraryPath();
                if (projectFile != null)
                {
                    TestProjectLibraryParameter = projectFile.FullName;
                    return;
                }
            }

            if (TestProjectLibraryParameter != null)
            {
                var file = Path.GetDirectoryName(TestProjectParameter).FindFile(TestProjectLibraryParameter);
                if (file != null)
                {
                    TestProjectLibraryParameter = file.FullName;
                    return;
                }
            }

            throw new MuTestInputException(ErrorMessage, $"Test Project Library (.dll) is not exist. {CliOptions.TestLib.ArgumentDescription}");
        }

        private void ValidateRequiredParameters()
        {
            if (string.IsNullOrWhiteSpace(SourceProjectParameter) ||
                !SourceProjectParameter.EndsWith(".csproj") ||
                !File.Exists(SourceProjectParameter))
            {
                throw new MuTestInputException(ErrorMessage, $"The Source Project File (.csproj) is required. Valid Options are {CliOptions.SourceProject.ArgumentShortName}");
            }

            if (string.IsNullOrWhiteSpace(TestProjectParameter) ||
                !TestProjectParameter.EndsWith(".csproj") ||
                !File.Exists(TestProjectParameter)
            )
            {
                throw new MuTestInputException(ErrorMessage, $"The Test Project File (.csproj) is required. Valid Options are {CliOptions.TestProject.ArgumentShortName}");
            }

            if (string.IsNullOrWhiteSpace(ProcessWholeProject))
            {
                if (string.IsNullOrWhiteSpace(ClassName) && !MultipleSourceClasses.Any())
                {
                    throw new MuTestInputException(ErrorMessage,
                        $"Source class file name or fully qualified name is required. Valid Options are {CliOptions.SourceClass.ArgumentShortName} or process whole project using {CliOptions.ProcessWholeProject.ArgumentShortName}");
                }

                if (string.IsNullOrWhiteSpace(TestClassParameter) && !MultipleTestClasses.Any())
                {
                    throw new MuTestInputException(ErrorMessage, $"Test class file name (.cs) is required. Valid Options are {CliOptions.TestClass.ArgumentShortName}");
                }

                if (string.IsNullOrWhiteSpace(TestClassParameter) &&
                    MultipleSourceClasses.Count != MultipleTestClasses.Count)
                {
                    throw new MuTestInputException(ErrorMessage,
                        $"Number of source Classes Should be equal to number of test classes Count[{CliOptions.MultipleSourceClasses.ArgumentShortName}] = Count[{CliOptions.MultipleTestClasses.ArgumentShortName}]");
                }
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