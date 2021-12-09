using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using MuTest.ProjectCreator.Utility;
using static MuTest.ProjectCreator.Utility.Constants;
using static MuTest.ProjectCreator.Utility.Placeholders;

namespace MuTest.ProjectCreator
{
    class Program
    {
        static void Main()
        {
            Directory.SetCurrentDirectory(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName ?? throw new InvalidOperationException());
            while (true)
            {
                Console.Write(EnterSolutionPath);
                var solutionFile = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(solutionFile) ||
                    !File.Exists(solutionFile) ||
                    !Path.GetExtension(solutionFile).Equals(SolutionExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine(InvalidSolutionFileErrorMessage);
                    continue;
                }

                Console.WriteLine("1. Show Solution Info?");
                Console.WriteLine("2. Generate Unit Test Projects?");
                Console.WriteLine("3. Generate .Net Core Unit Test Projects?");
                Console.Write("Select Action: ");

                int.TryParse(Console.ReadLine(), out var action);

                if (action == 1)
                {
                    var projects = GetAllProjects(solutionFile);

                    Console.Write(EnterTestProjectFormat);
                    var format = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(format) || !format.Contains(SourceProjectFormatPlaceholder))
                    {
                        Console.WriteLine(InvalidFormatErrorMessage);
                        continue;
                    }

                    var matchedSourceProjects = new List<ProjectInSolution>();
                    var matchedTestProjects = new List<ProjectInSolution>();

                    foreach (var project in projects)
                    {
                        var testProject = projects.FirstOrDefault(x => x.ProjectName.Equals(
                            format.Replace(SourceProjectFormatPlaceholder,
                                project.ProjectName),
                            StringComparison.InvariantCultureIgnoreCase));

                        if (testProject != null)
                        {
                            matchedSourceProjects.Add(project);
                            matchedTestProjects.Add(testProject);
                        }
                    }

                    var unmatchedTestPorjects = projects
                        .Except(matchedTestProjects)
                        .Except(matchedSourceProjects)
                        .Where(x => x.ProjectName.EndsWith(TestProjectExtension) ||
                                    x.ProjectName.EndsWith(UnitTestProjectExtension) ||
                                    x.ProjectName.EndsWith(TestsProjectExtension)).ToList();

                    var unmatchedSourceProjects = projects
                        .Except(matchedSourceProjects)
                        .Except(matchedTestProjects)
                        .Except(unmatchedTestPorjects)
                        .ToList();

                    Console.WriteLine($"Source Projects Following Standard Format `{format}`");
                    matchedSourceProjects.ForEach(x=> Console.WriteLine(x.ProjectName));

                    Console.WriteLine();
                    Console.WriteLine($"Source Projects not Following Standard Format `{format}`");
                    unmatchedSourceProjects.ForEach(x => Console.WriteLine(x.ProjectName));

                    Console.WriteLine();
                    Console.WriteLine($"Test Projects not Following Standard Format `{format}`");
                    unmatchedTestPorjects.ForEach(x => Console.WriteLine(x.ProjectName));

                    break;
                }

                if (action == 2)
                {
                    Console.Write(EnterUnitTestsOutputPath);
                    var unitTestsDir = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(unitTestsDir) ||
                        !Directory.Exists(unitTestsDir))
                    {
                        Console.WriteLine(DirectoryNotExistErrorMessage);
                        continue;
                    }

                    Console.Write(EnterTemplateName);
                    var template = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(template) ||
                        !Directory.Exists(Path.Combine(Templates, template)))
                    {
                        Console.WriteLine(TemplateNotExistErrorMessage);
                        continue;
                    }

                    Console.Write(EnterCompanyName);
                    var company = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(company))
                    {
                        Console.WriteLine(EmptyCompanyNameErrorMessage);
                        continue;
                    }

                    Console.Write(EnterPackagesPath);
                    var packagesPath = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(packagesPath))
                    {
                        Console.WriteLine(InvalidPackagesPathErrorMessage);
                        continue;
                    }

                    Console.Write(EnterNUnitVersion);
                    var nunitVersion = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(nunitVersion) ||
                        !Regex.IsMatch(nunitVersion, SemanticVersionRegex))
                    {
                        Console.WriteLine(InvalidNunitVersion);
                        continue;
                    }

                    Console.Write(EnterNUnitAdapterVersion);
                    var nunitAdapterVersion = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(nunitAdapterVersion) ||
                        !Regex.IsMatch(nunitAdapterVersion, SemanticVersionRegex))
                    {
                        Console.WriteLine(InvalidNunitAdapterVersion);
                        continue;
                    }

                    Console.Write(EnterTestProjectFormat);
                    var format = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(format) || !format.Contains(SourceProjectFormatPlaceholder))
                    {
                        Console.WriteLine(InvalidFormatErrorMessage);
                        continue;
                    }

                    var templateDir = new DirectoryInfo(Path.Combine(Path.Combine(Templates, template)));
                    var files = templateDir
                        .GetFiles("*.*", SearchOption.AllDirectories)
                        .ToList();

                    var assemblyFile = files.FirstOrDefault(x => x.Name == AssemblyInfoClass);
                    var packagesFile = files.FirstOrDefault(x => x.Name == PackagesFile);
                    var projectFile = files.FirstOrDefault(x => x.Name.EndsWith(CSharpProjectExtension));
                    var assemblyInfo = assemblyFile?.GetCodeFileContent();
                    var projectInfo = projectFile?.GetCodeFileContent();
                    var packagesInfo = packagesFile?.GetCodeFileContent();

                    files = files.Where(x => x.Name != AssemblyInfoClass &&
                                             !x.Name.EndsWith(CSharpProjectExtension)).ToList();

                    if (string.IsNullOrWhiteSpace(assemblyInfo) || string.IsNullOrWhiteSpace(projectInfo))
                    {
                        Console.WriteLine(InvalidTemplateErrorMessage);
                        continue;
                    }

                    var projects = GetProjects(solutionFile);
                    foreach (var project in projects)
                    {
                        var testProject = format.Replace(SourceProjectFormatPlaceholder, project.ProjectName);
                        var projectDir = Path.Combine(unitTestsDir, testProject);
                        if (Directory.Exists(projectDir))
                        {
                            continue;
                        }

                        var directoryInfo = Directory.CreateDirectory(projectDir);
                        Directory.CreateDirectory(Path.Combine(directoryInfo.FullName, PropertiesFolder));
                        var guid = Guid.NewGuid().ToString().ToUpper(CultureInfo.InvariantCulture);
                        var sourceProjectLib = new FileInfo(project.AbsolutePath).FindLibraryPathWithoutValidation()?.Name;

                        foreach (var fileInfo in files)
                        {
                            var destFileName = Path.Combine(directoryInfo.FullName, fileInfo.FullName.Replace(templateDir.FullName, string.Empty).Trim('\\'));
                            var content = fileInfo.GetCodeFileContent();
                            content = content.Replace(GuidPlaceholder, guid);
                            content = content.Replace(TestProjectPlaceholder, testProject);
                            content = content.Replace(SourceProjectPlaceholder, project.ProjectName);
                            content = content.Replace(SourceProjectGuidPlaceholder, project.ProjectGuid);
                            content = content.Replace(SourceProjectRelativePathPlaceholder, project.RelativePath);
                            content = content.Replace(SourceProjectLibraryPlaceholder, sourceProjectLib);
                            content = content.Replace(CompanyPlaceholder, company);

                            var newFile = new FileInfo(destFileName);
                            newFile.Create().Close();
                            newFile.FullName.WriteLines(new List<string> { content });
                        }

                        var projectAssembly = assemblyInfo.Replace(GuidPlaceholder, guid);
                        projectAssembly = projectAssembly.Replace(TestProjectPlaceholder, testProject);
                        projectAssembly = projectAssembly.Replace(CompanyPlaceholder, company);
                        var assembly = new FileInfo(Path.Combine(directoryInfo.FullName, PropertiesFolder, AssemblyInfoClass));
                        assembly.Create().Close();
                        assembly.FullName.WriteLines(new List<string> { projectAssembly });

                        var projectData = projectInfo.Replace(GuidPlaceholder, guid);
                        projectData = projectData.Replace(TestProjectPlaceholder, testProject);
                        projectData = projectData.Replace(PackagesPathPlaceholder, packagesPath);
                        projectData = projectData.Replace(SourceProjectPlaceholder, project.ProjectName);
                        projectData = projectData.Replace(SourceProjectGuidPlaceholder, project.ProjectGuid);
                        projectData = projectData.Replace(SourceProjectRelativePathPlaceholder, project.RelativePath);
                        projectData = projectData.Replace(NunitVersionPlaceholder, nunitVersion);
                        projectData = projectData.Replace(NunitAdapterVersionPlaceholder, nunitAdapterVersion);
                        var proj = new FileInfo(Path.Combine(directoryInfo.FullName, $"{testProject}.csproj"));
                        proj.Create().Close();
                        proj.FullName.WriteLines(new List<string> { projectData });

                        var packagesData = packagesInfo?.Replace(NunitVersionPlaceholder, nunitVersion);
                        packagesData = packagesData?.Replace(NunitAdapterVersionPlaceholder, nunitAdapterVersion);
                        var package = new FileInfo(Path.Combine(directoryInfo.FullName, PackagesFile));
                        package.Create().Close();
                        package.FullName.WriteLines(new List<string> { packagesData });
                        Console.WriteLine($"Generated Tests Projects at {unitTestsDir}");
                    }

                    break;
                }

                if (action == 3)
                {
                    Console.Write(EnterUnitTestsOutputPath);
                    var unitTestsDir = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(unitTestsDir) ||
                        !Directory.Exists(unitTestsDir))
                    {
                        Console.WriteLine(DirectoryNotExistErrorMessage);
                        continue;
                    }

                    Console.Write(EnterTemplateName);
                    var template = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(template) ||
                        !Directory.Exists(Path.Combine(Templates, template)))
                    {
                        Console.WriteLine(TemplateNotExistErrorMessage);
                        continue;
                    }

                    Console.Write(EnterTestProjectFormat);
                    var format = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(format) || !format.Contains(SourceProjectFormatPlaceholder))
                    {
                        Console.WriteLine(InvalidFormatErrorMessage);
                        continue;
                    }

                    var templateDir = new DirectoryInfo(Path.Combine(Path.Combine(Templates, template)));
                    var files = templateDir
                        .GetFiles("*.*", SearchOption.AllDirectories)
                        .ToList();

                    var projectFile = files.FirstOrDefault(x => x.Name.EndsWith(CSharpProjectExtension));
                    var projectInfo = projectFile?.GetCodeFileContent();

                    files = files.Where(x => x.Name != AssemblyInfoClass &&
                                             !x.Name.EndsWith(CSharpProjectExtension)).ToList();

                    if (string.IsNullOrWhiteSpace(projectInfo))
                    {
                        Console.WriteLine(InvalidTemplateErrorMessage);
                        continue;
                    }

                    var projects = GetProjects(solutionFile);
                    foreach (var project in projects)
                    {
                        var testProject = format.Replace(SourceProjectFormatPlaceholder, project.ProjectName);
                        var projectDir = Path.Combine(unitTestsDir, testProject);
                        if (Directory.Exists(projectDir))
                        {
                            continue;
                        }

                        var directoryInfo = Directory.CreateDirectory(projectDir);
                        Directory.CreateDirectory(Path.Combine(directoryInfo.FullName, PropertiesFolder));

                        foreach (var fileInfo in files)
                        {
                            var destFileName = Path.Combine(directoryInfo.FullName, fileInfo.FullName.Replace(templateDir.FullName, string.Empty).Trim('\\'));
                            var content = fileInfo.GetCodeFileContent();
                            content = content.Replace(TestProjectPlaceholder, testProject);
                            content = content.Replace(SourceProjectPlaceholder, project.ProjectName);
                            content = content.Replace(SourceProjectRelativePathPlaceholder, project.RelativePath);
                            content = content.Replace(SourceProjectLibraryPlaceholder, $"{project.ProjectName}.dll");

                            var newFile = new FileInfo(destFileName);
                            newFile.Create().Close();
                            newFile.FullName.WriteLines(new List<string> { content });
                        }

                        var projectData = projectInfo.Replace(TestProjectPlaceholder, testProject);
                        projectData = projectData.Replace(SourceProjectPlaceholder, project.ProjectName);
                        projectData = projectData.Replace(SourceProjectRelativePathPlaceholder, project.RelativePath);
                        var proj = new FileInfo(Path.Combine(directoryInfo.FullName, $"{testProject}.csproj"));
                        proj.Create().Close();
                        proj.FullName.WriteLines(new List<string> { projectData });

                        Console.WriteLine($"Generated Tests Projects at {unitTestsDir}");
                    }

                    break;
                }
            }
        }

        private static IEnumerable<ProjectInSolution> GetProjects(string solutionFile)
        {
            var sol = new FileInfo(solutionFile);
            var projects = SolutionFile.Parse(sol.FullName)
                .ProjectsInOrder
                .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat &&
                            !x.ProjectName.EndsWith(CommonFakes) &&
                            Path.GetExtension(x.AbsolutePath) == CSharpProjectExtension &&
                            !x.ProjectName.EndsWith(TestProjectExtension) &&
                            !x.ProjectName.EndsWith(UnitTestProjectExtension) &&
                            !x.ProjectName.EndsWith(TestsProjectExtension)).ToList();
            return projects;
        }

        private static IList<ProjectInSolution> GetAllProjects(string solutionFile)
        {
            var sol = new FileInfo(solutionFile);
            var projects = SolutionFile.Parse(sol.FullName)
                .ProjectsInOrder
                .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat &&
                            !x.ProjectName.EndsWith(CommonFakes) &&
                            Path.GetExtension(x.AbsolutePath) == CSharpProjectExtension).ToList();
            return projects;
        }
    }
}
