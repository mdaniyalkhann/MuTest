using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MuTest.Core.Model.CoverageAnalysis;
using MuTest.Core.Testing;
using MuTest.Core.Utility;
using MuTest.CoverageAnalyzer.Common;
using MuTest.CoverageAnalyzer.Model.Json;
using MuTest.CoverageAnalyzer.Options;
using Newtonsoft.Json;
using Class = MuTest.CoverageAnalyzer.Model.Json.Class;
using Project = MuTest.CoverageAnalyzer.Model.Json.Project;

namespace MuTest.CoverageAnalyzer
{
    internal class CoverageAnalyzerRunner : ICoverageAnalyzerRunner
    {
        private Chalk _chalk;
        private static readonly ClassCoverageAnalyzerFactory ClassCoverageAnalyzerFactory = new ClassCoverageAnalyzerFactory();
        private static readonly IClassExtractor[] ClassExtractors = { new MsBuildClassExtractor(), new NoMSBuildCoverageProcessor() };

        public void RunAnalyzer(CoverageAnalyzerOptions options)
        {
            _chalk = new Chalk();

            foreach (var solution in options.Solutions)
            {
                _chalk.Yellow($"Processing Solution `{solution.FullName}` Projects\n");
                var solutionName = Path.GetFileNameWithoutExtension(solution.FullName);
                foreach (Model.Project sourceProject in solution.SourceProjects)
                {
                    var sourceProjectFile = new FileInfo(sourceProject.AbsolutePath);
                    var sourceProjectLibraryPath = sourceProjectFile.FindLibraryPathWithoutValidation(options.ProjectConfigurationParameter);
                    var solutionRelativePath = solution.FullName.RelativePath(options.CodePathParameter);

                    var testProject = solution.TestProjects.FirstOrDefault(
                        x => x.ProjectName.Equals(
                            options.TestProjectMapFormat.Replace(
                                Placeholder.ProjectName,
                                sourceProject.ProjectName), StringComparison.InvariantCultureIgnoreCase));
                    var sourceClasses = new List<Class>();
                    var testProjectLib = testProject != null
                            ? new FileInfo(testProject.AbsolutePath).FindLibraryPathWithoutValidation(
                                options.ProjectConfigurationParameter)
                            : null;
                    var assemblyName = sourceProjectFile.GetAssemblyName();
                    var assemblyCreationDate = GetUnderTestAssemblyCreationDateTime(testProjectLib, assemblyName);
                    var allClasses = ExtractClasses(sourceProject);
                    foreach (var claz in allClasses)
                    {
                        _chalk.Cyan($"Finding paths of class `{claz.FullClassName}`\n");
                        var classRelativePath = claz.FilePath.RelativePath(options.CodePathParameter);

                        var partialClass = FindPartial(sourceClasses, claz);
                        if (partialClass != null)
                        {
                            partialClass.ClassPaths.Add(classRelativePath);
                            continue;
                        }
                        var clz = new Class(options.CodePathParameter)
                        {
                            ClassName = claz.FullClassName,
                            ClassPaths = new List<string> { classRelativePath }
                        };
                        sourceClasses.Add(clz);
                    }
                    var classCoverageAnalyzer = ClassCoverageAnalyzerFactory.Create(options);
                    foreach (var sourceClass in sourceClasses)
                    {
                        _chalk.Cyan($"Extracting coverage for class `{sourceClass.ClassName}`\n");
                        var findResult = classCoverageAnalyzer.TryFindCoverage(sourceClass.ClassName,
                            sourceClass.ClassPaths, assemblyName, assemblyCreationDate, out var coverage);
                        if (findResult != FindCoverageResult.Found || coverage.TotalLines == 0)
                        {
                            continue;
                        }

                        var coveredRatio = (decimal)coverage.LinesCovered / coverage.TotalLines;
                        sourceClass.Hut = new Hut
                        {
                            LinesCoveredCount = coverage.LinesCovered,
                            LinesCoveredRatio = Math.Round(coveredRatio, 3),
                            TotalLines = coverage.TotalLines
                        };
                    }

                    sourceClasses = sourceClasses.Where(s => s.Hut != null).ToList();

                    var project = new Project
                    {
                        Branch = options.BranchParameter,
                        CommonFakesProject = solution.CommonFakesProject?.AbsolutePath?.RelativePath(options.CodePathParameter),
                        FakesContainerProject = options.FakesContainerProject?.RelativePath(options.CodePathParameter),
                        FakesContainerGenerated = options.FakesContainerAssemblies?.RelativePath(options.CodePathParameter),
                        ProjectId = options.ProjectIdParameter,
                        RepositoryName = options.RepositoryParameter,
                        SourceLibrary = sourceProjectLibraryPath?.FullName.RelativePath(options.CodePathParameter),
                        TestLibrary = testProjectLib?.FullName.RelativePath(options.CodePathParameter),
                        SourcePath = sourceProject.AbsolutePath.RelativePath(options.CodePathParameter),
                        TestPath = testProject?.AbsolutePath.RelativePath(options.CodePathParameter),
                        Classes = sourceClasses.ToArray(),
                        Solutions = new[] { solutionRelativePath }
                    };

                    if (!string.IsNullOrWhiteSpace(solutionName))
                    {
                        var branchFolderName = options.BranchParameter.Replace("/", "_");
                        var dir = Path.Combine(
                            options.OutputDirectoryParameter,
                            branchFolderName,
                            "solutions",
                            solutionName);

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var fullPath = Path.Combine(dir, $"{sourceProject.ProjectName}.json");

                        File.WriteAllText(fullPath,
                            JsonConvert.SerializeObject(project, Formatting.Indented));

                        var jsonFile = new FileInfo(fullPath);
                        _chalk.Default($"\nYour json report has been generated at: \nPath: {jsonFile.FullName} \n");
                    }
                }
            }
        }

        private static DateTime? GetUnderTestAssemblyCreationDateTime(FileInfo testProjectLib, string underTestAssemblyName)
        {
            var underTestAssemblyPath = testProjectLib?.Directory.GetFiles(underTestAssemblyName).FirstOrDefault();
            return underTestAssemblyPath != null ? File.GetLastWriteTime(underTestAssemblyPath.FullName) : (DateTime?)null;
        }

        private static IEnumerable<Model.Class> ExtractClasses(Model.Project sourceProject)
        {
            foreach (var extractor in ClassExtractors)
            {
                try
                {
                    return extractor.ExtractClasses(sourceProject);
                }
                catch
                {
                    continue;
                }
            }
            throw new InvalidOperationException($"Could not extract classes from project {sourceProject.AbsolutePath}");
        }

        private static Class FindPartial(IList<Class> classes, Model.Class claz)
        {
            return classes.FirstOrDefault(c => c.ClassName == claz.FullClassName);
        }
    }
}