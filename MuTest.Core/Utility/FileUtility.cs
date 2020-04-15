using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace MuTest.Core.Utility
{
    public static class FileUtility
    {
        /// <summary>
        /// Gets c# files Info
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileInfo[] GetCSharpFileInfos(string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles("*.cs", SearchOption.AllDirectories)
                    .Where(x => x.Name.EndsWith(".cs", StringComparison.CurrentCulture) &&
                                !x.Name.StartsWith("f.") &&
                                !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                !x.Name.StartsWith("AssemblyInfo")).ToArray();
            }

            return null;
        }

        public static IList<string> GetCodeCoverages(this string path)
        {
            if (Directory.Exists(path))
            {
                var directoryInfo = new DirectoryInfo(path);
                var coverageFiles = directoryInfo.EnumerateFiles("*.coverage", SearchOption.AllDirectories)
                    .Where(x => x.Name.EndsWith(".coverage", StringComparison.CurrentCulture) &&
                                !x.Name.StartsWith("f.") &&
                                !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                !x.Name.StartsWith("AssemblyInfo")).ToList();

                coverageFiles.AddRange(directoryInfo.EnumerateFiles("*.coveragexml", SearchOption.AllDirectories)
                    .Where(x => x.Name.EndsWith(".coveragexml", StringComparison.CurrentCulture) &&
                                !x.Name.StartsWith("f.") &&
                                !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                !x.Name.StartsWith("AssemblyInfo")).ToList());

                return coverageFiles.OrderByDescending(x => x.LastWriteTime).Select(x => x.FullName).ToList();
            }

            return null;
        }

        public static FileInfo FindFile(this string path, string fileName)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles(fileName, SearchOption.AllDirectories)
                    .FirstOrDefault(x => x.Name.EndsWith(fileName, StringComparison.InvariantCulture) &&
                                         !x.Name.StartsWith("f.") &&
                                         !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                         !x.Name.StartsWith("AssemblyInfo"));
            }

            return null;
        }

        public static FileInfo FindProjectFile(this string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => x.Name.EndsWith(".csproj", StringComparison.InvariantCulture) &&
                                         !x.Name.StartsWith("f.") &&
                                         !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                         !x.Name.StartsWith("AssemblyInfo"));
            }

            return null;
        }

        public static FileInfo FindProjectFile(this FileInfo file)
        {
            var projectFile = file.DirectoryName.FindProjectFile();
            if (projectFile == null)
            {
                var parentDirectory = file.Directory?.Parent;
                while (parentDirectory != null)
                {
                    projectFile = parentDirectory.FullName.FindProjectFile();
                    if (projectFile != null)
                    {
                        break;
                    }

                    parentDirectory = parentDirectory.Parent;
                }
            }

            return projectFile;
        }

        public static FileInfo[] FindSolutionFiles(this string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles("*.sln", SearchOption.AllDirectories)
                    .Where(x => x.Name.EndsWith(".sln", StringComparison.CurrentCulture) &&
                                !x.Name.StartsWith("f.") &&
                                !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                !x.Name.StartsWith("AssemblyInfo")).ToArray();
            }

            return null;
        }

        public static FileInfo FindLibraryPath(this FileInfo project, string configuration = "Debug")
        {
            if (project != null && project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var assembly = projectXml.SelectSingleNode("/Project/PropertyGroup/AssemblyName")?.InnerText ?? Path.GetFileNameWithoutExtension(project.Name);
                var outputPath = projectXml.SelectSingleNode("/Project/PropertyGroup/OutputPath")?.InnerText ?? Path.Combine("bin", "Debug");
                var targetPlatform = projectXml.SelectSingleNode("/Project/PropertyGroup/TargetFramework");
                if (!string.IsNullOrWhiteSpace(project.DirectoryName))
                {
                    outputPath = outputPath.Replace("$(Configuration)", configuration);
                    var sourceDllPath = Path.GetFullPath(Path.Combine(project.DirectoryName, outputPath));
                    var library = sourceDllPath.FindFile($"{assembly}.dll");
                    library = library ?? sourceDllPath.FindFile($"{assembly}.exe");

                    if (library == null && targetPlatform != null)
                    {
                        library = Path.Combine(sourceDllPath, targetPlatform.InnerText).FindFile($"{assembly}.dll");
                        library = library ?? Path.Combine(sourceDllPath, targetPlatform.InnerText).FindFile($"{assembly}.exe");
                    }

                    return library;
                }
            }

            return null;
        }

        public static bool DoNetCoreProject(this string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return false;
            }

            var project = new FileInfo(projectPath);
            if (project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var targetPlatform = projectXml.SelectSingleNode("/Project/PropertyGroup/TargetFramework");

                if (targetPlatform != null)
                {
                    return targetPlatform.InnerText.StartsWith("netcoreapp", StringComparison.InvariantCultureIgnoreCase);
                }
            }

            return false;
        }

        public static IList<string> GetProjectThirdPartyLibraries(this string projectPath)
        {
            var libs = new List<string>();
            if (string.IsNullOrWhiteSpace(projectPath))
            {
                return libs;
            }

            var project = new FileInfo(projectPath);
            if (project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var references = projectXml.SelectNodes("/Project/ItemGroup/Reference/HintPath");

                if (references != null)
                {
                    foreach (XmlNode reference in references)
                    {
                        libs.Add(reference.InnerText);
                    }
                }
            }

            return libs;
        }

        public static string UpdateTestProject(this string projectPath, string testClassName)
        {
            var newPath = projectPath;
            var project = new FileInfo(projectPath);
            if (project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var references = projectXml.SelectNodes("/Project/ItemGroup/Compile");

                if (references != null)
                {
                    newPath = Path.Combine(project.DirectoryName, $"{Path.GetFileNameWithoutExtension(project.FullName)}.mutest.csproj");
                    for (var index = 0; index < references.Count; index++)
                    {
                        XmlNode reference = references[index];
                        var include = reference.Attributes?["Include"];
                        if (include != null)
                        {
                            var innerText = include.InnerText;
                            if (!Regex.IsMatch(innerText, $@"{testClassName}.cs|{testClassName}\..*\.cs", RegexOptions.IgnoreCase) &&
                                Regex.IsMatch(innerText, @".*Test.cs|.*Tests.cs|.*Test\..*\.cs|.*Tests\..*\.cs"))
                            {
                                reference.ParentNode?.RemoveChild(reference);
                            }
                        }
                    }

                    var newPathFile = new FileInfo(newPath);
                    if (newPathFile.Exists)
                    {
                        newPathFile.Delete();
                    }

                    projectXml.Save(newPathFile.FullName);

                    return newPathFile.FullName;
                }
            }


            return newPath;
        }

        public static FileInfo FindLibraryPathWithoutValidation(this FileInfo project, string configuration = "Debug")
        {
            if (project != null && project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var assembly = projectXml.SelectSingleNode("/Project/PropertyGroup/AssemblyName");
                var outputPath = projectXml.SelectSingleNode("/Project/PropertyGroup/OutputPath");
                var outputType = GetOutputType(projectXml);

                if (assembly != null &&
                    outputPath != null &&
                    !string.IsNullOrWhiteSpace(project.DirectoryName))
                {
                    var outputPathText = outputPath.InnerText;
                    outputPathText = outputPathText.Replace("$(Configuration)", configuration);
                    var sourceDllPath = Path.GetFullPath(Path.Combine(project.DirectoryName, outputPathText));

                    return new FileInfo(Path.Combine(sourceDllPath, $"{assembly.InnerText}{outputType}"));
                }
            }

            return null;
        }

        private static string GetOutputType(XmlNode projectXml)
        {
            var outputTypeNode = projectXml.SelectSingleNode("/Project/PropertyGroup/OutputType");
            var outputType = outputTypeNode != null
                                 && (outputTypeNode.InnerText.Equals("Exe", StringComparison.InvariantCultureIgnoreCase) || outputTypeNode.InnerText.Equals("WinExe", StringComparison.InvariantCultureIgnoreCase))
                    ? ".exe"
                    : ".dll";
            return outputType;
        }

        public static string GetAssemblyName(this FileInfo project)
        {
            var projectXml = project.GetProjectDocument();
            var assembly = projectXml.SelectSingleNode("/Project/PropertyGroup/AssemblyName");
            var outputType = GetOutputType(projectXml);
            return $"{assembly?.InnerText}{outputType}";
        }

        public static NameValueCollection GetProjectFiles(this FileInfo project)
        {
            var dictionary = new NameValueCollection();
            if (project != null &&
                project.Exists)
            {
                var projectXml = project.GetProjectDocument();
                var classes = projectXml.SelectNodes("/Project/ItemGroup/Compile[@Include]/@Include");
                if (classes != null &&
                    project.DirectoryName != null)
                {
                    foreach (XmlNode path in classes)
                    {
                        var relativePath = path.InnerText;
                        var classPath = Path.GetFullPath(Path.Combine(project.DirectoryName, relativePath));
                        if (dictionary[relativePath] == null)
                        {
                            dictionary.Add(relativePath, classPath);
                        }
                    }
                }
            }

            return dictionary;
        }

        public static XmlDocument GetProjectDocument(this FileSystemInfo project)
        {
            var projectXmlFile = File.ReadAllText(project.FullName);
            projectXmlFile = Regex.Replace(projectXmlFile, "xmlns=.*\"", string.Empty);
            var projectXml = new XmlDocument();
            projectXml.LoadXml(projectXmlFile);

            return projectXml;
        }

        /// <summary>
        ///  Get Code File Content
        /// </summary>
        /// <param name="info">File Info</param>
        /// <returns></returns>
        public static string GetCodeFileContent(this FileInfo info)
        {
            return new StringBuilder().Append(File.ReadAllText(info.FullName)).ToString();
        }

        public static string GetCodeFileContent(this string file)
        {
            return GetCodeFileContent(new FileInfo(file));
        }

        public static void DirectoryCopy(this string sourceDirName, string destDirName, string extensionsToCopy = "")
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
            }

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = string.IsNullOrWhiteSpace(extensionsToCopy)
                ? dir.EnumerateFiles().ToList()
                : dir.EnumerateFiles().Where(x => extensionsToCopy.Contains(x.Extension)).ToList();
            foreach (var file in files)
            {
                var combine = Path.Combine(destDirName, file.Name);
                file.CopyTo(combine, true);
            }

            foreach (var subDirectory in dirs)
            {
                var combine = Path.Combine(destDirName, subDirectory.Name);
                DirectoryCopy(subDirectory.FullName, combine);
            }
        }

        public static string RelativePath(this string absolutePath, string root)
        {
            if (string.IsNullOrWhiteSpace(absolutePath) || string.IsNullOrWhiteSpace(root))
            {
                throw new ArgumentNullException($"{nameof(absolutePath)} and ${nameof(root)} is required!");
            }

            if (!root.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                root += Path.DirectorySeparatorChar;
            }

            var fromUri = new Uri(root);
            var toUri = new Uri(absolutePath);

            if (fromUri.Scheme != toUri.Scheme)
            {
                return root;
            }

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static void WriteLines(this string path, IList<string> lines)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (lines == null)
            {
                throw new ArgumentNullException(nameof(lines));
            }

            using (var writer = new StreamWriter(path))
            {
                foreach (var fileLine in lines)
                {
                    writer.WriteLine(fileLine);
                }
            }
        }

        public static bool IsSubdirectoryOf(this string dir1, string dir2)
        {
            var di1 = new DirectoryInfo(dir1.TrimEnd(Path.DirectorySeparatorChar));
            var di2 = new DirectoryInfo(dir2.TrimEnd(Path.DirectorySeparatorChar));
            while (di1.Parent != null)
            {
                if (di1.Parent.FullName == di2.FullName)
                {
                    return true;
                }

                di1 = di1.Parent;
            }

            return false;
        }

        public static string GetExactPathName(this string pathName)
        {
            if (!(File.Exists(pathName) || Directory.Exists(pathName)))
                return pathName;

            var di = new DirectoryInfo(pathName);

            if (di.Parent != null)
            {
                return Path.Combine(
                    GetExactPathName(di.Parent.FullName),
                    di.Parent.GetFileSystemInfos(di.Name)[0].Name);
            }
            else
            {
                return di.Name.ToUpper();
            }

        }
    }
}