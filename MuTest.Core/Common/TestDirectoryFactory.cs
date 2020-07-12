using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MuTest.Core.Model;
using MuTest.Core.Utility;

namespace MuTest.Core.Common
{
    public class TestDirectoryFactory : ITestDirectoryFactory
    {
        public int NumberOfMutantsExecutingInParallel { get; set; } = 5;

        public string BuildExtensions { get; set; }

        private readonly SourceClassDetail _source;

        public TestDirectoryFactory(SourceClassDetail source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public void DeleteDirectories()
        {
            Reset();
            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                var directory = $"{Path.GetDirectoryName(_source.TestClaz.ClassLibrary)}_test_{index}";
                if (Directory.Exists(directory))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Trace.TraceError("Skipping as one or multiples files are in use {0}", e.StackTrace);
                    }
                }
            }

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                var directory = $"{Path.GetDirectoryName(_source.ClassLibrary)}{index}";
                if (Directory.Exists(directory))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Trace.TraceError("Skipping as one or multiples files are in use {0}", e.StackTrace);
                    }
                }
            }

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                var duplicateCodeFile = GetSourceCodeFile(index);
                if (duplicateCodeFile.Exists)
                {
                    duplicateCodeFile.Delete();
                }

                var duplicateProjectFile = GetProjectFile(index);
                if (duplicateProjectFile.Exists)
                {
                    duplicateProjectFile.Delete();
                }
            }
        }

        private static void Reset()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public FileInfo GetSourceCodeFile(int index)
        {
            return new FileInfo($"{Path.GetDirectoryName(_source.FilePath)}\\{Path.GetFileNameWithoutExtension(_source.FilePath)}_mutest_{index}{Path.GetExtension(_source.FilePath)}");
        }

        public FileInfo GetSourceCodeFile(int index, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            return new FileInfo($"{Path.GetDirectoryName(path)}\\{Path.GetFileNameWithoutExtension(path)}_mutest_{index}{Path.GetExtension(path)}");
        }

        public FileInfo GetProjectFile(int index)
        {
            return new FileInfo($"{Path.GetDirectoryName(_source.ClassProject)}\\{Path.GetFileNameWithoutExtension(_source.ClassProject)}_mutest_{index}{Path.GetExtension(_source.ClassProject)}");
        }

        public FileInfo GetProjectFile(int index, string project)
        {
            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            return new FileInfo($"{Path.GetDirectoryName(project)}\\{Path.GetFileNameWithoutExtension(project)}_mutest_{index}{Path.GetExtension(project)}");
        }

        public async Task PrepareDirectoriesAndFiles()
        {
            try
            {
                Reset();
                for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
                {
                    var testDirectory = Path.GetDirectoryName(_source.TestClaz.ClassLibrary);
                    testDirectory.DirectoryCopy($"{testDirectory}_test_{index}");
                }

                for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
                {
                    var sourceDirectory = Path.GetDirectoryName(_source.ClassLibrary);
                    sourceDirectory.DirectoryCopy($"{sourceDirectory}{index}", BuildExtensions);
                }

                for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
                {
                    var sourceDirectory = Path.GetDirectoryName(_source.ClassLibrary);
                    Directory.CreateDirectory($@"{sourceDirectory}{index}\obj\");
                }

                for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
                {
                    await UpdateProjectFile(index, _source.Claz.Syntax.NameSpace(), _source.FilePath, _source.ClassProject);
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to Prepare Project Files and Directories {0}", exp);
            }
        }

        public async Task UpdateProjectFile(int index, string classNamespace, string path, string project)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            var sourceCodeFile = new FileInfo(path);
            var duplicateCodeFile = GetSourceCodeFile(index, path);
            sourceCodeFile.CopyTo(duplicateCodeFile.FullName, true);

            var duplicateProjectFile = GetProjectFile(index, project);
            var relativePath = sourceCodeFile.FullName.RelativePath(new FileInfo(project).DirectoryName);

            if (!_source.DoNetCoreProject)
            {
                var projectXml = File.ReadAllText(project);
                var nameSpace = classNamespace;
                if (nameSpace != null && nameSpace.Contains("."))
                {
                    nameSpace = nameSpace.Split('.').Last();
                }

                var match = Regex.Match(projectXml, $@"\<Compile Include=\""{relativePath.Replace("\\", "\\\\")}", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    match = Regex.Match(projectXml, $@"\<Compile Include=\"".*{nameSpace}\\{Path.GetFileName(sourceCodeFile.FullName)}", RegexOptions.IgnoreCase);
                }

                if (!match.Success)
                {
                    match = Regex.Match(projectXml, $@"\<Compile Include=\"".*\\{Path.GetFileName(sourceCodeFile.FullName)}", RegexOptions.IgnoreCase);
                }

                if (!match.Success)
                {
                    match = Regex.Match(projectXml, $@"\<Compile Include=\""{Path.GetFileName(sourceCodeFile.FullName)}", RegexOptions.IgnoreCase);
                }

                if (!match.Success)
                {
                    match = Regex.Match(projectXml, $@"\<Compile Include=\"".*{Path.GetFileName(sourceCodeFile.FullName)}", RegexOptions.IgnoreCase);
                }

                var projectFileContent = projectXml.Replace(
                    match.Value,
                    match.Value
                        .Replace($"\\{Path.GetFileName(sourceCodeFile.FullName)}", $"\\{Path.GetFileName(duplicateCodeFile.FullName)}")
                        .Replace($"\"{Path.GetFileName(sourceCodeFile.FullName)}", $"\"{Path.GetFileName(duplicateCodeFile.FullName)}"));

                if (projectXml.Equals(projectFileContent, StringComparison.InvariantCultureIgnoreCase))
                {
                    projectFileContent = projectXml.Replace(
                        match.Value,
                        Regex.Replace(Regex.Replace(match.Value
                                , $@"\\{Path.GetFileName(sourceCodeFile.FullName)}", $@"\{Path.GetFileName(duplicateCodeFile.FullName)}", RegexOptions.IgnoreCase)
                            , $@"""{Path.GetFileName(sourceCodeFile.FullName)}", $@"""{Path.GetFileName(duplicateCodeFile.FullName)}", RegexOptions.IgnoreCase));
                }

                File.Create(duplicateProjectFile.FullName).Close();
                using (var outputFile = new StreamWriter(duplicateProjectFile.FullName))
                {
                    await outputFile.WriteAsync(projectFileContent);
                }
            }
            else
            {
                var projectXml = new FileInfo(project).GetProjectDocument();
                var projectNode = projectXml.SelectSingleNode("Project");
                var targetPropertyGroup = projectXml.SelectSingleNode("/Project/PropertyGroup/TargetFramework")?.ParentNode;
                var assemblyNode = projectXml.SelectSingleNode("/Project/PropertyGroup/AssemblyName");

                if (projectNode != null && 
                    targetPropertyGroup != null)
                {
                    if (assemblyNode == null)
                    {
                        targetPropertyGroup.InnerXml += $"<AssemblyName>{Path.GetFileNameWithoutExtension(project)}</AssemblyName>";
                    }
                    var xml = string.Join(string.Empty,
                        projectNode.InnerXml,
                        "<ItemGroup>",
                        $"<Compile Remove=\"{relativePath}\"/>");

                    for (var fileIndex = 0; fileIndex < NumberOfMutantsExecutingInParallel; fileIndex++)
                    {
                        if (fileIndex != index)
                        {
                            var dupFileRelativePath = GetSourceCodeFile(fileIndex, path).FullName.RelativePath(new FileInfo(project).DirectoryName);
                            xml = string.Join(string.Empty,
                                xml,
                                $"<Compile Remove=\"{dupFileRelativePath}\"/>");
                        }
                    }

                    xml = string.Join(string.Empty,
                        xml,
                        "</ItemGroup>");

                    projectNode.InnerXml = xml;
                }

                projectXml.Save(duplicateProjectFile.FullName);
            }
        }
    }
}
