using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuTest.Console.Model;
using MuTest.Console.Options;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Exceptions;
using MuTest.Core.Model;
using MuTest.Core.Testing;
using MuTest.Core.Utility;
using Newtonsoft.Json;
using static MuTest.Core.Common.Constants;

namespace MuTest.Console
{
    public class MuTestRunner : IMuTestRunner
    {
        private const string ProjectSummary = "Project_Summary";
        public static readonly MuTestSettings MuTestSettings = MuTestSettingsSection.GetSettings();

        public IMutantExecutor MutantExecutor { get; private set; }

        private SourceClassDetail _source;
        private IChalk _chalk;
        private MuTestOptions _options;
        private Stopwatch _stopwatch;

        public async Task RunMutationTest(MuTestOptions options)
        {
            if (!File.Exists(MuTestSettings.MSBuildPath))
            {
                throw new MuTestInputException($"Unable to locate MSBuild Path at {MuTestSettings.MSBuildPath}. Please update MSBuildPath in MuTest.Console.exe.config if you are using different version");
            }

            if (!File.Exists(MuTestSettings.VSTestConsolePath))
            {
                throw new MuTestInputException($"Unable to locate VS Test Console Path at {MuTestSettings.VSTestConsolePath}. Please update VSTestConsolePath in MuTest.Console.exe.config if you are using different version");
            }

            if (!File.Exists(MuTestSettings.RunSettingsPath))
            {
                throw new MuTestInputException($"Unable to locate tests run settings path at {MuTestSettings.RunSettingsPath}. Please update RunSettingsPath in MuTest.Console.exe.config if you are using different location");
            }

            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            _chalk = new Chalk();
            _options = options;

            if (!_options.SkipTestProjectBuild)
            {
                var originalProject = _options.TestProjectParameter;
                if (_options.OptimizeTestProject && 
                    _options.MultipleTargetClasses.Count == 1)
                {
                    var targetClass = _options.MultipleTargetClasses.First();

                    
                    _options.TestProjectParameter = _options
                        .TestProjectParameter
                        .UpdateTestProject(targetClass.TestClassPath.GetClass().ClassName());
                }

                await ExecuteBuild();

                if (!originalProject.Equals(_options.TestProjectParameter, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (File.Exists(_options.TestProjectParameter))
                    {
                        File.Delete(_options.TestProjectParameter);
                    }

                    _options.TestProjectParameter = originalProject;
                }
            }

            var serial = 1;
            _chalk.Default("\n*********************************** Matched Classes ***********************************");
            foreach (var srcClass in _options.MultipleTargetClasses)
            {
                _chalk.Green($"\n{serial++}. {srcClass.ClassName} in {srcClass.ClassPath}");
            }

            _chalk.Default("\n***************************************************************************************");

            var projectSummary = new ProjectSummary
            {
                SourceProject = _options.SourceProjectParameter,
                TestProject = _options.TestProjectParameter
            };

            var mutantAnalyzer = new MutantAnalyzer(_chalk, MuTestSettings)
            {
                ProcessWholeProject = _options.ProcessWholeProject,
                BuildInReleaseMode = _options.BuildInReleaseModeParameter,
                ConcurrentTestRunners = _options.ConcurrentTestRunners,
                EnableDiagnostics = _options.EnableDiagnostics,
                ExecuteAllTests = _options.ExecuteAllTests,
                IncludeNestedClasses = _options.IncludeNestedClasses,
                IncludePartialClasses = _options.IncludePartialClasses || 
                                        _options.UseClassFilter || 
                                        _options.ExecuteAllTests,
                KilledThreshold = _options.KilledThreshold,
                NoCoverage = _options.NoCoverage,
                RegEx = _options.RegEx,
                Specific = _options.Specific,
                SurvivedThreshold = _options.SurvivedThreshold,
                TestProject = _options.TestProjectParameter,
                TestProjectLibrary = _options.TestProjectLibraryParameter,
                UseClassFilter = _options.UseClassFilter,
                X64TargetPlatform = _options.X64TargetPlatform,
                TestExecutionTime = _options.TestExecutionThreshold,
                MutantsPerLine = _options.MutantsPerLine
            };

            foreach (var targetClass in _options.MultipleTargetClasses)
            {
                mutantAnalyzer.TestClass = targetClass.TestClassPath;
                mutantAnalyzer.UseExternalCodeCoverage = false;
                mutantAnalyzer.MutantExecutor = null;
                var sourceClass = targetClass.ClassPath;
                var className = targetClass.ClassName;
                mutantAnalyzer.SourceProjectLibrary = _options.SourceProjectLibraryParameter;

                try
                {
                    mutantAnalyzer.ExternalCoveredMutants.Clear();
                    _source = await mutantAnalyzer.Analyze(sourceClass, className, _options.SourceProjectParameter);

                    if (_source.ExternalCoveredClassesIncluded.Any() && _options.AnalyzeExternalCoveredClasses)
                    {
                        _chalk.Yellow("\n\nAnalyzing External Coverage...");
                        mutantAnalyzer.UseExternalCodeCoverage = true;
                        foreach (var acc in _source.ExternalCoveredClassesIncluded)
                        {
                            mutantAnalyzer.ExternalCoveredMutants.AddRange(acc.MutantsLines);
                            var projectFile = new FileInfo(acc.ClassPath).FindProjectFile();
                            mutantAnalyzer.SourceProjectLibrary = projectFile.FindLibraryPath()?.FullName;
                            if (!string.IsNullOrWhiteSpace(mutantAnalyzer.SourceProjectLibrary))
                            {
                                var accClass = await mutantAnalyzer.Analyze(
                                    acc.ClassPath,
                                    acc.ClassName,
                                    projectFile.FullName);

                                accClass.CalculateMutationScore();

                                if (accClass.MutationScore.Survived == 0)
                                {
                                    acc.ZeroSurvivedMutants = true;
                                }
                            }
                        }

                        mutantAnalyzer.ExternalCoveredMutants.Clear();
                    }
                }
                catch (Exception ex) when (!(ex is MuTestInputException))
                {
                    throw;
                }
                finally
                {
                    MutantExecutor = mutantAnalyzer.MutantExecutor;
                    _stopwatch.Stop();
                    if (_source != null && MutantExecutor != null)
                    {
                        GenerateReports();

                        if (!string.IsNullOrWhiteSpace(_options.ProcessWholeProject))
                        {
                            projectSummary.Classes.Add(new ClassSummary
                            {
                                TargetClass = new TargetClass
                                {
                                    ClassPath = _source.FilePath,
                                    ClassName = _source.Claz.Syntax.FullName(),
                                    TestClassPath = _source.TestClaz.FilePath
                                },
                                MutationScore = _source.MutationScore,
                                Coverage = _source.Coverage ?? Coverage.Create(0, 0, 0, 0)
                            });
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(_options.ProcessWholeProject))
            {
                projectSummary.CalculateMutationScore();
                var builder = new StringBuilder(HtmlTemplate);
                builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
                builder.AppendLine("Mutation Report".PrintImportantWithLegend());
                builder.Append("  ".PrintWithPreTag());
                builder.Append($"{"Source Project:".PrintImportant()} {projectSummary.SourceProject}".PrintWithPreTag());
                builder.Append($"{"Test Project  :".PrintImportant()} {projectSummary.TestProject}".PrintWithPreTag());
                builder.Append("  ".PrintWithPreTag());

                builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
                builder.AppendLine("Classes Mutation".PrintImportantWithLegend());
                foreach (var claz in projectSummary.Classes)
                {
                    builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");

                    builder.AppendLine($"{claz.TargetClass.ClassName} [{claz.TargetClass.ClassPath}]".PrintImportantWithLegend(color: Colors.BlueViolet));
                    builder.Append($"{claz.MutationScore.Mutation} - {claz.MutationScore}".PrintWithPreTagWithMarginImportant());
                    builder.Append($"Code Coverage - {claz.Coverage}".PrintWithPreTagWithMarginImportant());

                    builder.AppendLine("</fieldset>");
                }

                builder.AppendLine("</fieldset>");

                builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
                builder.AppendLine("ProjectWise Summary".PrintImportantWithLegend());
                builder.Append(projectSummary.MutationScore.ToString().PrintWithPreTagWithMarginImportant(color: Colors.BlueViolet));
                builder.Append($"Coverage: Mutation({projectSummary.MutationScore.Mutation}) {projectSummary.Coverage}".PrintWithPreTagWithMarginImportant(color: Colors.Blue));
                builder.AppendLine("</fieldset>");
                builder.AppendLine("</fieldset>");

                CreateHtmlReport(builder, ProjectSummary);
                CreateJsonReport(ProjectSummary, projectSummary);
            }
        }

        private async Task ExecuteBuild()
        {
            _chalk.Default("Building Test Project...");
            var log = new StringBuilder();
            void OutputData(object sender, string args) => log.AppendLine(args);
            var testCodeBuild = new BuildExecutor(MuTestSettings, _options.TestProjectParameter)
            {
                BaseAddress = MuTestSettings.ServiceAddress,
                EnableLogging = _options.EnableDiagnostics,
                QuietWithSymbols = true
            };
            testCodeBuild.OutputDataReceived += OutputData;
            if (_options.BuildInReleaseModeParameter)
            {
                if (_options.OptimizeTestProject)
                {
                    await testCodeBuild.ExecuteBuildInReleaseModeWithoutDependencies();
                }
                else
                {
                    await testCodeBuild.ExecuteBuildInReleaseModeWithDependencies();
                }
            }
            else
            {
                if (_options.OptimizeTestProject)
                {
                    await testCodeBuild.ExecuteBuildInDebugModeWithoutDependencies();
                }
                else
                {
                    await testCodeBuild.ExecuteBuildInDebugModeWithDependencies();
                }
            }

            testCodeBuild.OutputDataReceived -= OutputData;

            if (testCodeBuild.LastBuildStatus == BuildExecutionStatus.Failed)
            {
                throw new MuTestFailingBuildException(log.ToString());
            }

            _chalk.Green("\nBuild Succeeded!");
        }

        private void GenerateReports()
        {
            var consoleBuilder = new StringBuilder();
            MutantExecutor.PrintMutatorSummary(consoleBuilder);
            MutantExecutor.PrintClassSummary(consoleBuilder);
            consoleBuilder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10;\">");
            consoleBuilder.AppendLine("Execution Time: ".PrintImportantWithLegend());
            consoleBuilder.Append($"{_stopwatch.Elapsed}".PrintWithPreTagWithMarginImportant());
            consoleBuilder.AppendLine("</fieldset>");

            _chalk.Default($"{Environment.NewLine}{consoleBuilder.ToString().ConvertToPlainText()}{Environment.NewLine}");

            _source.ExecutionTime = _stopwatch.ElapsedMilliseconds;
            var builder = new StringBuilder(HtmlTemplate);
            builder.Append(MutantExecutor?.LastExecutionOutput.PrintImportant() ?? string.Empty);
            MutantExecutor?.PrintMutatorSummary(builder);
            MutantExecutor?.PrintClassSummary(builder);
            builder.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10;\">");
            builder.AppendLine("Execution Time: ".PrintImportantWithLegend());
            builder.Append($"{_stopwatch.Elapsed}".PrintWithPreTagWithMarginImportant());
            builder.AppendLine("</fieldset>");

            _source.FullName = _source.Claz.Syntax.FullName();
            CreateHtmlReport(builder, _source.FullName.Replace(".", "_"));
            CreateJsonReport(
                _source.FullName.Replace(".", "_"),
                new JsonOptions
                {
                    Options = _options,
                    Result = _source
                });
        }

        private void CreateJsonReport<T>(string fileName, T output)
        {
            if (!string.IsNullOrWhiteSpace(_options.JsonOutputPath))
            {
                var outputPath = _options.JsonOutputPath.Replace(SourceClassPlaceholder, fileName);
                var file = new FileInfo(outputPath);
                if (file.Exists)
                {
                    file.Delete();
                }

                var directoryName = Path.GetDirectoryName(file.FullName);
                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                file.Create().Close();
                File.WriteAllText(outputPath, JsonConvert.SerializeObject(output, Formatting.Indented));

                _chalk.Green($"\nYour json report has been generated at: \n {file.FullName} \n");
            }
        }

        private void CreateHtmlReport(StringBuilder builder, string fileName)
        {
            if (!string.IsNullOrWhiteSpace(_options.HtmlOutputPath))
            {
                var outputPath = _options.HtmlOutputPath.Replace(SourceClassPlaceholder, fileName);
                var file = new FileInfo(outputPath);
                if (file.Exists)
                {
                    file.Delete();
                }

                var directoryName = Path.GetDirectoryName(file.FullName);
                if (!string.IsNullOrWhiteSpace(directoryName) &&
                    !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                file.Create().Close();
                File.WriteAllText(outputPath, builder.ToString());

                _chalk.Green($"\nYour html report has been generated at: \n {file.FullName} \nYou can open it in your browser of choice. \n");
            }
        }
    }
}