using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Build.Construction;
using MuTest.Core.Utility;

namespace MuTest.Cpp.CLI.Utility
{
    public static class CppFileUtility
    {
        public static FileInfo FindCppProjectFile(this FileInfo file)
        {
            var projectFile = file.DirectoryName.FindCppProjectFile();
            if (projectFile == null)
            {
                var parentDirectory = file.Directory?.Parent;
                while (parentDirectory != null)
                {
                    projectFile = parentDirectory.FullName.FindCppProjectFile();
                    if (projectFile != null)
                    {
                        break;
                    }

                    parentDirectory = parentDirectory.Parent;
                }
            }

            return projectFile;
        }

        private static FileInfo FindCppProjectFile(this string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles("*.vcxproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => x.Name.EndsWith(".vcxproj", StringComparison.InvariantCulture) &&
                                         !x.Name.StartsWith("f.") &&
                                         !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                         !x.Name.StartsWith("AssemblyInfo"));
            }

            return null;
        }

        public static FileInfo FindCppSolutionFile(this FileInfo file, string testProject)
        {
            var projectFile = file.DirectoryName.FindCppSolutionFile(testProject);
            if (projectFile == null)
            {
                var parentDirectory = file.Directory?.Parent;
                while (parentDirectory != null)
                {
                    projectFile = parentDirectory.FullName.FindCppSolutionFile(testProject);
                    if (projectFile != null)
                    {
                        break;
                    }

                    parentDirectory = parentDirectory.Parent;
                }
            }

            return projectFile;
        }

        public static void ReplaceLine(this string originalFile, int lineNumber, string newLine, string destinationFolder)
        {
            if (string.IsNullOrWhiteSpace(originalFile))
            {
                throw new ArgumentNullException(nameof(originalFile));
            }

            if (string.IsNullOrWhiteSpace(newLine))
            {
                throw new ArgumentNullException(nameof(newLine));
            }

            if (string.IsNullOrWhiteSpace(destinationFolder))
            {
                throw new ArgumentNullException(nameof(destinationFolder));
            }

            var lines = new List<string>();
            using (var reader = new StreamReader(originalFile))
            {
                var lineIndex = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineIndex++;
                    if (lineNumber == lineIndex)
                    {
                        lines.Add(newLine);
                        continue;
                    }

                    lines.Add(line);
                }
            }

            destinationFolder.WriteLines(lines);
        }

        public static void DeleteIfExists(this FileSystemInfo file)
        {
            if (file == null)
            {
                return;
            }

            if (file.Exists)
            {
                file.Delete();
            }
        }

        public static void DeleteIfExists(this string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var file = new FileInfo(path);

            if (file.Exists)
            {
                file.Delete();
            }
        }

        public static void UpdateCode(this string updatedSourceCode, string codeFile)
        {
            if (codeFile == null)
            {
                throw new ArgumentNullException(nameof(codeFile));
            }

            while (true)
            {
                try
                {
                    if (File.Exists(codeFile))
                    {
                        File.Delete(codeFile);
                    }

                    File.Create(codeFile).Close();

                    using (var outputFile = new StreamWriter(codeFile))
                    {
                        outputFile.Write(updatedSourceCode);
                    }

                    break;
                }
                catch (Exception ex)
                {
                    Trace.TraceError("Exception suppressed: {0}", ex);
                    Debug.WriteLine("File is inaccessible....Try again");
                }
            }
        }

        public static void AddNameSpace(this string codeFile, int index)
        {
            if (codeFile == null)
            {
                throw new ArgumentNullException(nameof(codeFile));
            }

            var fileLines = new List<string>();
            using (var reader = new StreamReader(codeFile))
            {
                string line;
                var namespaceAdded = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("#") ||
                        line.Trim().StartsWith("//") ||
                        string.IsNullOrWhiteSpace(line) ||
                        namespaceAdded)
                    {
                        fileLines.Add(line);
                        continue;
                    }

                    fileLines.Add($"namespace mutest_test_{index} {{ {Environment.NewLine}{Environment.NewLine}");
                    fileLines.Add(line);
                    namespaceAdded = true;
                }

                fileLines.Add("}");
            }

            codeFile.WriteLines(fileLines);
        }

        public static void UpdateTestProject(this string newProjectLocation, string originalClassName, string newClassName)
        {
            var project = new FileInfo(newProjectLocation);
            if (project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var references = projectXml.SelectNodes("/Project/ItemGroup/ClCompile");

                if (references != null)
                {
                    for (var index = 0; index < references.Count; index++)
                    {
                        XmlNode reference = references[index];
                        var include = reference.Attributes?["Include"];
                        if (include != null)
                        {
                            var innerText = include.InnerText;
                            if (Regex.IsMatch(innerText, originalClassName, RegexOptions.IgnoreCase))
                            {
                                var itemGroup = reference.ParentNode;
                                if (itemGroup != null)
                                {
                                    newClassName.AddClCompileNode(projectXml, itemGroup);
                                    itemGroup.RemoveChild(reference);
                                    break;
                                }
                            }

                            if (index == references.Count - 1)
                            {
                                var itemGroup = reference.ParentNode;
                                if (itemGroup != null)
                                {
                                    newClassName.AddClCompileNode(projectXml, itemGroup);
                                }
                            }
                        }
                    }

                    var newPathFile = new FileInfo(newProjectLocation);
                    if (newPathFile.Exists)
                    {
                        newPathFile.Delete();
                    }

                    projectXml.Save(newPathFile.FullName);
                }
            }
        }

        public static void OptimizeTestProject(this string newProjectLocation)
        {
            const string warningLevel = "/Project/ItemDefinitionGroup/ClCompile/WarningLevel";
            const string wholeProgramOptimization = "/Project/PropertyGroup/WholeProgramOptimization";
            const string linkIncremental = "/Project/PropertyGroup/LinkIncremental";
            const string generateManifest = "GenerateManifest";
            const string debugInformationFormat = "DebugInformationFormat";
            const string supportJustMyCode = "SupportJustMyCode";
            const string multiProcessorCompilation = "MultiProcessorCompilation";
            const string errorReporting = "ErrorReporting";
            const string none = "None";
            const string turnOffAllwarnings = "TurnOffAllWarnings";
            const string optimization = "/Project/ItemDefinitionGroup/ClCompile/Optimization";
            const string trueValue = "true";
            const string disabled = "Disabled";
            const string generateDebugInformation = "/Project/ItemDefinitionGroup/Link/GenerateDebugInformation";
            const string linkErrorReporting = "LinkErrorReporting";
            const string noErrorReport = "NoErrorReport";

            var project = new FileInfo(newProjectLocation);
            if (project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                projectXml.SetInnerTextMultipleNodes(wholeProgramOptimization);
                projectXml.SetInnerTextMultipleNodes(linkIncremental);
                projectXml.SetInnerTextMultipleNodes(generateDebugInformation);

                projectXml.AddNewXmlNode(linkIncremental, generateManifest);

                projectXml.SetInnerTextMultipleNodes(warningLevel, turnOffAllwarnings);
                projectXml.AddNewXmlNode(warningLevel, debugInformationFormat, none);
                projectXml.AddNewXmlNode(warningLevel, supportJustMyCode);
                projectXml.AddNewXmlNode(warningLevel, multiProcessorCompilation, trueValue);
                projectXml.AddNewXmlNode(warningLevel, errorReporting, none);
                projectXml.AddNewXmlNode(generateDebugInformation, linkErrorReporting, noErrorReport);

                projectXml.SetInnerTextMultipleNodes(optimization, disabled);

                var newPathFile = new FileInfo(newProjectLocation);
                if (newPathFile.Exists)
                {
                    newPathFile.Delete();
                }

                projectXml.Save(newPathFile.FullName);
            }
        }

        private static void SetInnerTextMultipleNodes(this XmlNode projectXml, string xmlPath, string text = "false")
        {
            var nodes = projectXml.SelectNodes(xmlPath);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    node.InnerText = text;
                }
            }
        }

        private static void AddNewXmlNode(this XmlDocument projectXml, string xmlPath, string element, string text = "false")
        {
            var nodes = projectXml.SelectNodes(xmlPath);
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    var xmlElement = projectXml.CreateElement(element);
                    xmlElement.InnerText = text;
                    var childNodes = node.ParentNode?.SelectNodes(element);
                    if (childNodes == null || childNodes.Count == 0)
                    {
                        node.ParentNode?.AppendChild(xmlElement);
                        continue;
                    }

                    foreach (XmlNode child in childNodes)
                    {
                        child.InnerText = text;
                    }
                }
            }
        }

        private static void AddClCompileNode(this string newClassName, XmlDocument projectXml, XmlNode itemGroup)
        {
            var includeAttribute = projectXml.CreateAttribute("Include");
            includeAttribute.InnerText = newClassName;
            var element = projectXml.CreateElement("ClCompile");
            element.Attributes.Append(includeAttribute);

            itemGroup.AppendChild(element);
        }

        private static FileInfo FindCppSolutionFile(this string path, string testProject)
        {
            if (Directory.Exists(path))
            {
                var solutions = new DirectoryInfo(path).EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)
                    .Where(x => x.Name.EndsWith(".sln", StringComparison.InvariantCulture) &&
                                !x.Name.StartsWith("f.") &&
                                !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                !x.Name.StartsWith("AssemblyInfo"));

                return (from solution in solutions
                        let projects = solution.FullName.GetProjects()
                        where projects.Where(project => project?.AbsolutePath != null)
                            .Any(project => Path.GetFileName(project.AbsolutePath).Equals(Path.GetFileName(testProject), StringComparison.InvariantCultureIgnoreCase))
                        select solution).FirstOrDefault();
            }

            return null;
        }

        public static IEnumerable<ProjectInSolution> GetProjects(this string solutionFile)
        {
            if (string.IsNullOrWhiteSpace(solutionFile) || !File.Exists(solutionFile))
            {
                return new List<ProjectInSolution>();
            }

            var sol = new FileInfo(solutionFile);
            var projects = SolutionFile.Parse(sol.FullName)
                .ProjectsInOrder
                .Where(x => x.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat ||
                            x.ProjectType == SolutionProjectType.SolutionFolder).ToList();
            return projects;
        }
    }
}