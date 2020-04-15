using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace MuTest.ProjectCreator.Utility
{
    public static class FileUtility
    {

        /// <summary>
        ///  Get Code File Content
        /// </summary>
        /// <param name="info">File Info</param>
        /// <returns></returns>
        public static string GetCodeFileContent(this FileInfo info)
        {
            return new StringBuilder().Append(File.ReadAllText(info.FullName)).ToString();
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

        private static XmlDocument GetProjectDocument(this FileSystemInfo project)
        {
            var projectXmlFile = File.ReadAllText(project.FullName);
            projectXmlFile = Regex.Replace(projectXmlFile, "xmlns=.*\"", string.Empty);
            var projectXml = new XmlDocument();
            projectXml.LoadXml(projectXmlFile);

            return projectXml;
        }
    }
}