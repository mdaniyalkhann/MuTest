using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.Coverage.Analysis;
using MuTest.Core.Common.CoverageAnalyzers;
using MuTest.Core.Exceptions;
using MuTest.Core.Model.CoverageReports;
using MuTest.Core.Utility;
using MuTest.CoverageAnalyzer.Model;
using Newtonsoft.Json;
using static MuTest.CoverageAnalyzer.Common.Placeholder;

namespace MuTest.CoverageAnalyzer.Options
{
    public class CoverageAnalyzerOptions
    {
        private const string ErrorMessage = "The value for one of your settings is not correct. Try correcting or removing them.";

        public string RepositoryParameter { get; set; }

        public string BranchParameter { get; set; } = "develop";

        public string CodePathParameter { get; set; }

        public string CodeCoverageParameter { get; set; }

        public string CoberturaReportRootFolderPath { get; set; }

        public string CodeCoverageType { get; set; } = CodeCoverageTypes.VsTest;
     
	    public string FakesContainerProject { get; set; }

        public string FakesContainerAssemblies { get; set; }

        public string CommonFakesProjectFormat { get; set; } = "CommonFakes.Tests";

        public string TestProjectMapFormat { get; set; } = $"{ProjectName}.Tests";

        public string OutputDirectoryParameter { get; set; }

        public List<string> SpecificSolutions { get; } = new List<string>();

        public List<string> ExcludeSolutions { get; } = new List<string>();

        public List<Solution> Solutions { get; } = new List<Solution>();

        public string ProjectIdParameter { get; set; }

        public List<CoverageDS> CodeCoverages { get; set; } = new List<CoverageDS>();

        public CoberturaCoverageReport CoberturaCoverageReport { get; set; }

        public int UncoveredThresholdParameter { get; set; } = 20;

        public int CoveredPercentageParameter { get; set; } = 90;

        public string ProjectConfigurationParameter { get; set; } = "Debug";

        public void ValidateOptions()
        {
            ValidateRequiredOptions();
            ValidateSolutions();
            ValidateCodeCoverage();
            SetOutputPath();
        }

        private void ValidateCodeCoverage()
        {
            var coverageFiles = new List<string>();
            if (string.IsNullOrWhiteSpace(CodeCoverageParameter))
            {
                coverageFiles.AddRange(CodePathParameter.GetCodeCoverages());
            }
            else
            {
                if (LoadCoberturaReport())
                {
                    return;
                }

                coverageFiles.Add(CodeCoverageParameter);
            }

            try
            {
                foreach (var coverageFile in coverageFiles)
                {
                    var extension = Path.GetExtension(coverageFile);
                    if (extension == ".coverage")
                    {
                        using (var coverageInfo = CoverageInfo.CreateFromFile(coverageFile))
                        {
                            CodeCoverages.Add(coverageInfo.BuildDataSet());
                        }
                    }

                    if (extension == ".coveragexml")
                    {
                        var dataSet = new CoverageDS();
                        dataSet.ImportXml(coverageFile);
                        CodeCoverages.Add(dataSet);
                    }
                }

                if (CodeCoverages.Any())
                {
                    return;
                }

            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
            }

            throw new MuTestInputException("Code coverage file(s) are not exist or invalid ! Please use visual studio .coverage or .coveragexml or cobertura file", CliOptions.CodeCoveragePath.ArgumentShortName);
        }

        private bool LoadCoberturaReport()
        {
            if (!CoberturaReportRootFolderPath.EndsWith("/"))
            {
                CoberturaReportRootFolderPath += "/";
            }
            var jsonText = File.ReadAllText(CodeCoverageParameter);
            try
            {
                var report = JsonConvert.DeserializeObject<CoberturaCoverageReport>(jsonText);
                AdjustCoberturaReportPaths(report.results.children);
                CoberturaCoverageReport = report;
                CodeCoverageType = CodeCoverageTypes.Cobertura;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Makes absolute paths contained in cobertura report relative
        /// </summary>
        /// <param name="children"></param>
        private void AdjustCoberturaReportPaths(IList<Child> children)
        {
            foreach (var child in children)
            {
                child.name = child.name.Replace(CoberturaReportRootFolderPath, string.Empty);
                AdjustCoberturaReportPaths(child.children);
            }
        }

        private void ValidateSolutions()
        {
            var files = CodePathParameter.FindSolutionFiles();

            if (files != null && files.Any())
            {
                if (SpecificSolutions.Any())
                {
                    files = files.Where(x => SpecificSolutions.Any(sp => x.Name == sp || x.Name == $"{sp}.sln")).ToArray();
                }
                else if (ExcludeSolutions.Any())
                {
                    files = files.Where(x => !ExcludeSolutions.Any(ex => x.Name == ex || x.Name == $"{ex}.sln")).ToArray();
                }

                if (files.Any())
                {
                    foreach (var solutionFileInfo in files)
                    {
                        var solutionName = Path.GetFileNameWithoutExtension(solutionFileInfo.Name);
                        var solutionFile = SolutionFile.Parse(solutionFileInfo.FullName);
                        var projects = solutionFile.ProjectsInOrder
                            .Where(p => p.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
                            .Where(p => new FileInfo(p.AbsolutePath).Extension == ".csproj")
                            .Where(p => p.AbsolutePath.IsSubdirectoryOf(CodePathParameter)) // Project should belong in the current repo
                            .ToList();
                        if (projects.Any())
                        {
                            var commonFakesProject = projects.FirstOrDefault(x =>
                                x.ProjectName.Equals(CommonFakesProjectFormat.Replace(SolutionName, solutionName),
                                    StringComparison.InvariantCultureIgnoreCase));

                            var testProjects = projects
                                .Where(x => projects.Any(p => x.ProjectName.Equals(
                                    TestProjectMapFormat.Replace($"{ProjectName}", p.ProjectName),
                                    StringComparison.InvariantCultureIgnoreCase))).ToList();

                            var sourceProjects = projects
                                .Except(testProjects).ToList();

                            if (!testProjects.Any())
                            {
                                throw new MuTestInputException($"Not any test project exists in solution ${solutionFileInfo.FullName} matching `${TestProjectMapFormat}` format. Add test projects or exclude this solution using -exs option");
                            }

                            var solution = new Solution
                            {
                                FullName = solutionFileInfo.FullName,
                                CommonFakesProject = commonFakesProject != null ? new Project
                                {
                                    ProjectName = commonFakesProject.ProjectName,
                                    AbsolutePath = commonFakesProject.AbsolutePath,
                                    RelativePath = commonFakesProject.RelativePath
                                } : null
                            };

                            if (solution.CommonFakesProject != null)
                            {
                                solution.CommonFakesProject.Solution = solution;
                            }
                            solution.TestProjects.AddRange(testProjects.Select(x => new Project
                            {
                                Solution = solution,
                                AbsolutePath = x.AbsolutePath,
                                ProjectName = x.ProjectName,
                                RelativePath = x.RelativePath
                            }));

                            solution.SourceProjects.AddRange(sourceProjects.Select(x => new Project
                            {
                                Solution = solution,
                                AbsolutePath = x.AbsolutePath,
                                ProjectName = x.ProjectName,
                                RelativePath = x.RelativePath
                            }));

                            Solutions.Add(solution);
                        }
                    }

                    if (Solutions.Any())
                    {
                        return;
                    }
                }
            }

            throw new MuTestInputException("No any solution found in code directory", CliOptions.CodePath.ArgumentShortName);
        }

        private void ValidateRequiredOptions()
        {
            if (string.IsNullOrWhiteSpace(CodePathParameter) || !Directory.Exists(CodePathParameter))
            {
                Required("Code directory", CliOptions.CodePath.ArgumentShortName);
            }

            if (string.IsNullOrWhiteSpace(RepositoryParameter))
            {
                Required("Repository Url", CliOptions.Repository.ArgumentShortName);
            }

            if (!string.IsNullOrWhiteSpace(FakesContainerProject) && !File.Exists(FakesContainerProject))
            {
                Required("Fakes Container Project path", CliOptions.FakesContainerProject.ArgumentShortName);
            }

            if (!string.IsNullOrWhiteSpace(FakesContainerAssemblies) && !Directory.Exists(FakesContainerAssemblies))
            {
                Required("Fakes Container Assemblies path", CliOptions.FakesContainerAssemblies.ArgumentShortName);
            }

            if (string.IsNullOrWhiteSpace(ProjectIdParameter))
            {
                Required("Project Id", CliOptions.ProjectId.ArgumentShortName);
            }

            if (string.IsNullOrWhiteSpace(ProjectConfigurationParameter))
            {
                Required("Project Configuration", CliOptions.ProjectId.ArgumentShortName);
            }
        }

        private void SetOutputPath()
        {
            if (string.IsNullOrWhiteSpace(OutputDirectoryParameter))
            {
                OutputDirectoryParameter = "Result";

                return;
            }

            try
            {
                if (!Directory.Exists(OutputDirectoryParameter))
                {
                    Directory.CreateDirectory(OutputDirectoryParameter);
                }

                return;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.StackTrace);
            }


            throw new MuTestInputException("Output Directory is invalid or not exist", CliOptions.OutputDirectory.ArgumentDescription);
        }

        private static void Required(string name, string argument)
        {
            throw new MuTestInputException(ErrorMessage, $"{name} is required. Valid Options are {argument}");
        }
    }
}