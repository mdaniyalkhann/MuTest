using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

        public static FileInfo FindCppSolutionFile(this FileInfo file)
        {
            var projectFile = file.DirectoryName.FindCppSolutionFile();
            if (projectFile == null)
            {
                var parentDirectory = file.Directory?.Parent;
                while (parentDirectory != null)
                {
                    projectFile = parentDirectory.FullName.FindCppSolutionFile();
                    if (projectFile != null)
                    {
                        break;
                    }

                    parentDirectory = parentDirectory.Parent;
                }
            }

            return projectFile;
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

        private static FileInfo FindCppSolutionFile(this string path)
        {
            if (Directory.Exists(path))
            {
                return new DirectoryInfo(path).EnumerateFiles("*.sln", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(x => x.Name.EndsWith(".sln", StringComparison.InvariantCulture) &&
                                         !x.Name.StartsWith("f.") &&
                                         !x.Name.StartsWith("TemporaryGeneratedFile_") &&
                                         !x.Name.StartsWith("AssemblyInfo"));
            }

            return null;
        }
    }
}