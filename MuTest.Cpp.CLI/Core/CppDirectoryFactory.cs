using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using MuTest.Core.Utility;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Utility;

namespace MuTest.Cpp.CLI.Core
{
    public class CppDirectoryFactory : ICppDirectoryFactory
    {
        private const string IntDirName = "mutest_int_dir/";
        private const string OutDirName = "mutest_out_dir/";
        private const string IntermediateOutputPathName = "mutest_obj_dir/";
        private const string BinDirName = "mutest_bin_dir/";

        public int NumberOfMutantsExecutingInParallel { get; set; } = 5;

        public CppBuildContext PrepareTestFiles(CppClass cppClass)
        {
            if (cppClass == null)
            {
                throw new ArgumentNullException(nameof(cppClass));
            }

            cppClass.Validate();
            Reset();

            var projectDirectory = Path.GetDirectoryName(cppClass.TestProject);

            var testProjectName = Path.GetFileNameWithoutExtension(cppClass.TestProject);
            var testProjectExtension = Path.GetExtension(cppClass.TestProject);

            var testSolutionName = Path.GetFileNameWithoutExtension(cppClass.TestSolution);
            var testSolutionExtension = Path.GetExtension(cppClass.TestSolution);

            var solution = cppClass.TestSolution.GetCodeFileContent();
            var test = cppClass.TestClass.GetCodeFileContent();
            var source = cppClass.SourceClass.GetCodeFileContent();

            var newTestProject = $"{testProjectName}_mutest_project{testProjectExtension}";
            var newTestSolution = $"{testSolutionName}_mutest_sln{testSolutionExtension}";

            var newTestProjectLocation = $"{projectDirectory}\\{newTestProject}";
            var newSolutionLocation = $"{Path.GetDirectoryName(cppClass.TestSolution)}\\{newTestSolution}";

            var solutionCode = Regex.Replace(solution, $"{testProjectName}{testProjectExtension}", newTestProject, RegexOptions.IgnoreCase);
            solutionCode.UpdateCode(newSolutionLocation);

            new FileInfo(cppClass.TestProject).CopyTo(newTestProjectLocation, true);

            var context = new CppBuildContext
            {
                IntDir = IntDirName,
                OutDir = OutDirName,
                IntermediateOutputPath = IntermediateOutputPathName,
                OutputPath = BinDirName,
                TestProject = new FileInfo(newTestProjectLocation),
                TestSolution = new FileInfo(newSolutionLocation)
            };

            DeleteDirectories(context);
            CreateDirectories(context);

            var sourceClassName = Path.GetFileNameWithoutExtension(cppClass.SourceClass);
            var sourceHeaderName = Path.GetFileNameWithoutExtension(cppClass.SourceHeader);
            var sourceClassExtension = Path.GetExtension(cppClass.SourceClass);
            var sourceHeaderExtension = Path.GetExtension(cppClass.SourceHeader);

            var testClassName = Path.GetFileNameWithoutExtension(cppClass.TestClass);
            var testClassExtension = Path.GetExtension(cppClass.TestClass);

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                try
                {
                    var testContext = new CppTestContext
                    {
                        Index = index

                    };

                    var newSourceClass = $"{sourceClassName}_mutest_src_{index}{sourceClassExtension}";
                    var newSourceHeader = $"{sourceHeaderName}_mutest_src_{index}{sourceHeaderExtension}";
                    var newTestClass = $"{testClassName}_mutest_test_{index}{testClassExtension}";

                    var testCode = Regex.Replace(test, $"{sourceClassName}{sourceClassExtension}", newSourceClass, RegexOptions.IgnoreCase);
                    testCode = Regex.Replace(testCode, testClassName, Path.GetFileNameWithoutExtension(newTestClass), RegexOptions.IgnoreCase);
                    testCode = Regex.Replace(testCode, $"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader, RegexOptions.IgnoreCase);

                    var newSourceClassLocation = $"{Path.GetDirectoryName(cppClass.SourceClass)}\\{newSourceClass}";
                    var newHeaderClassLocation = $"{Path.GetDirectoryName(cppClass.SourceHeader)}\\{newSourceHeader}";
                    var newTestClassLocation = $"{Path.GetDirectoryName(cppClass.TestClass)}\\{newTestClass}";

                    newSourceClassLocation.DeleteIfExists();
                    newHeaderClassLocation.DeleteIfExists();
                    newTestClassLocation.DeleteIfExists();

                    testContext.SourceClass = new FileInfo(newSourceClassLocation);
                    testContext.SourceHeader = new FileInfo(newHeaderClassLocation);

                    if (!sourceHeaderExtension.Equals(sourceClassExtension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var sourceCode = Regex.Replace(source, $"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader, RegexOptions.IgnoreCase);
                        sourceCode.UpdateCode(newSourceClassLocation);
                        new FileInfo(cppClass.SourceHeader).CopyTo(newHeaderClassLocation, true);

                        context.NamespaceAdded = true;
                        newHeaderClassLocation.AddNameSpace(index);
                        newSourceClassLocation.AddNameSpace(index);
                    }
                    else
                    {
                        new FileInfo(cppClass.SourceClass).CopyTo(newSourceClassLocation);
                    }

                    testCode.UpdateCode(newTestClassLocation);
                    testContext.TestClass = new FileInfo(newTestClassLocation);

                    if (!Regex.IsMatch(testCode, testContext.SourceClass.Name, RegexOptions.IgnoreCase))
                    {
                        AddNameSpaceWithSourceReference(newTestClassLocation, testContext, index);
                    }
                    else
                    {
                        newTestClassLocation.AddNameSpace(index);
                    }

                    var relativeTestCodePath = cppClass.TestClass.RelativePath(projectDirectory);
                    var relativeNewTestCodePath = newTestClassLocation.RelativePath(projectDirectory);

                    context.TestProject.FullName.UpdateTestProject(relativeTestCodePath, relativeNewTestCodePath);
                    context.TestContexts.Add(testContext);
                }
                catch (Exception exp)
                {
                    context.TestContexts.Clear();
                    Console.WriteLine($"Unable to prepare Cpp Test Directories: {exp.Message}");
                    Trace.TraceError(exp.ToString());
                }
            }

            return context;
        }

        public CppBuildContext PrepareSolutionFiles(CppClass cppClass)
        {
            if (cppClass == null)
            {
                throw new ArgumentNullException(nameof(cppClass));
            }

            cppClass.Validate();
            Reset();

            var projectDirectory = Path.GetDirectoryName(cppClass.TestProject);

            var testProjectName = Path.GetFileNameWithoutExtension(cppClass.TestProject);
            var testProjectExtension = Path.GetExtension(cppClass.TestProject);

            var testSolutionName = Path.GetFileNameWithoutExtension(cppClass.TestSolution);
            var testSolutionExtension = Path.GetExtension(cppClass.TestSolution);

            var solution = cppClass.TestSolution.GetCodeFileContent();
            var test = cppClass.TestClass.GetCodeFileContent();
            var project = cppClass.TestProject.GetCodeFileContent();

            var newTestProject = $"{testProjectName}_mutest_project_{{0}}{testProjectExtension}";
            var newTestSolution = $"{testSolutionName}_mutest_sln_{{0}}{testSolutionExtension}";

            var newTestProjectLocation = $"{projectDirectory}\\{newTestProject}";
            var newSolutionLocation = $"{Path.GetDirectoryName(cppClass.TestSolution)}\\{newTestSolution}";

            var context = new CppBuildContext
            {
                IntDir = "mutest_int_dir_{0}/",
                OutDir = "mutest_out_dir_{0}/",
                IntermediateOutputPath = "mutest_obj_dir_{0}/",
                OutputPath = "mutest_bin_dir_{0}/",
                TestProject = new FileInfo(newTestProjectLocation),
                TestSolution = new FileInfo(newSolutionLocation),
                UseMultipleSolutions = true
            };

            DeleteDirectories(context);

            var sourceClassName = Path.GetFileNameWithoutExtension(cppClass.SourceClass);
            var sourceClassExtension = Path.GetExtension(cppClass.SourceClass);

            var testClassName = Path.GetFileNameWithoutExtension(cppClass.TestClass);
            var testClassExtension = Path.GetExtension(cppClass.TestClass);
            var source = cppClass.SourceClass.GetCodeFileContent();
            var header = string.Empty;
            var sourceHeaderName = string.Empty;
            var sourceHeaderExtension = string.Empty;

            if (!string.IsNullOrWhiteSpace(cppClass.SourceHeader) && File.Exists(cppClass.SourceHeader))
            {
                header = cppClass.SourceHeader.GetCodeFileContent();
                sourceHeaderName = Path.GetFileNameWithoutExtension(cppClass.SourceHeader);
                sourceHeaderExtension = Path.GetExtension(cppClass.SourceHeader);
            }

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                try
                {
                    CreateDirectories(context, index);
                    var testContext = new CppTestContext
                    {
                        Index = index
                    };

                    var newSourceClass = $"{sourceClassName}_mutest_src_{index}{sourceClassExtension}";

                    var newTestClass = $"{testClassName}_mutest_test_{index}{testClassExtension}";

                    var solutionCode = Regex.Replace(solution, $"{testProjectName}{testProjectExtension}", string.Format(newTestProject, index), RegexOptions.IgnoreCase);
                    solutionCode.UpdateCode(string.Format(newSolutionLocation, index));

                    var newSourceClassLocation = $"{Path.GetDirectoryName(cppClass.SourceClass)}\\{newSourceClass}";
                    var newTestClassLocation = $"{Path.GetDirectoryName(cppClass.TestClass)}\\{newTestClass}";

                    var relativeTestCodePath = cppClass.TestClass.RelativePath(projectDirectory);
                    var relativeNewTestCodePath = newTestClassLocation.RelativePath(projectDirectory);

                    var projectXml = Regex.Replace(project, relativeTestCodePath, relativeNewTestCodePath, RegexOptions.IgnoreCase);
                    projectXml = Regex.Replace(projectXml, $"{sourceClassName}{sourceClassExtension}", newSourceClass, RegexOptions.IgnoreCase);
                    projectXml.UpdateCode(string.Format(newTestProjectLocation, index));

                    newSourceClassLocation.DeleteIfExists();
                    newTestClassLocation.DeleteIfExists();
                    testContext.SourceClass = new FileInfo(newSourceClassLocation);

                    var testCode = Regex.Replace(test, $"{sourceClassName}{sourceClassExtension}", newSourceClass, RegexOptions.IgnoreCase);

                    if (!sourceHeaderExtension.Equals(sourceClassExtension, StringComparison.InvariantCultureIgnoreCase) &&
                        !string.IsNullOrWhiteSpace(header) &&
                        Regex.IsMatch(header, $"{sourceClassName}{sourceClassExtension}", RegexOptions.IgnoreCase))
                    {
                        var newSourceHeader = $"{sourceHeaderName}_mutest_src_{index}{sourceHeaderExtension}";
                        var newHeaderClassLocation = $"{Path.GetDirectoryName(cppClass.SourceHeader)}\\{newSourceHeader}";
                        testCode = Regex.Replace(testCode, $"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader, RegexOptions.IgnoreCase);
                        newHeaderClassLocation.DeleteIfExists();

                        var sourceCode = Regex.Replace(source, $"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader, RegexOptions.IgnoreCase);
                        sourceCode.UpdateCode(newSourceClassLocation);

                        var headerCode = Regex.Replace(header, $"{sourceClassName}{sourceClassExtension}", newSourceClass, RegexOptions.IgnoreCase);
                        headerCode.UpdateCode(newHeaderClassLocation);
                    }
                    else
                    {
                        new FileInfo(cppClass.SourceClass).CopyTo(newSourceClassLocation);
                    }

                    testCode = Regex.Replace(testCode, testClassName, Path.GetFileNameWithoutExtension(newTestClass), RegexOptions.IgnoreCase);

                    testCode.UpdateCode(newTestClassLocation);
                    testContext.TestClass = new FileInfo(newTestClassLocation);

                    if (!Regex.IsMatch(testCode, testContext.SourceClass.Name, RegexOptions.IgnoreCase) &&
                        !Regex.IsMatch(project, $"include.*=.*{sourceClassName}{sourceClassExtension}", RegexOptions.IgnoreCase))
                    {
                        AddSourceReference(testContext);
                    }

                    context.TestContexts.Add(testContext);
                }
                catch (Exception exp)
                {
                    context.TestContexts.Clear();
                    Console.WriteLine($"Unable to prepare Cpp Solution Files: {exp.Message}");
                    Trace.TraceError(exp.ToString());
                }
            }

            return context;
        }

        public void DeleteTestFiles(CppBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Reset();

            DeleteDirectories(context);
            context.TestProject.DeleteIfExists();
            context.TestSolution.DeleteIfExists();

            for (var index = 0; index < context.TestContexts.Count; index++)
            {
                var testContext = context.TestContexts[index];
                testContext.SourceClass.DeleteIfExists();
                testContext.SourceHeader.DeleteIfExists();
                testContext.TestClass.DeleteIfExists();

                string.Format(context.TestProject.FullName, index).DeleteIfExists();
                string.Format(context.TestSolution.FullName, index).DeleteIfExists();
            }
        }

        private static void DeleteDirectories(CppBuildContext context)
        {
            var testProjectDirectory = context.TestProject.Directory;
            if (testProjectDirectory != null &&
                testProjectDirectory.Exists)
            {
                testProjectDirectory.DeleteIfExists(IntDirName);
                testProjectDirectory.DeleteIfExists(OutDirName);
                testProjectDirectory.DeleteIfExists(IntermediateOutputPathName);
                testProjectDirectory.DeleteIfExists(BinDirName);
            }
        }

        private static void CreateDirectories(CppBuildContext context, int index = 0)
        {
            var testProjectDirectory = context.TestProject.Directory;
            if (testProjectDirectory != null &&
                testProjectDirectory.Exists)
            {
                Directory.CreateDirectory(Path.Combine(testProjectDirectory.FullName, string.Format(context.IntDir, index)));
                Directory.CreateDirectory(Path.Combine(testProjectDirectory.FullName, string.Format(context.OutDir, index)));
                Directory.CreateDirectory(Path.Combine(testProjectDirectory.FullName, string.Format(context.IntermediateOutputPath, index)));
                Directory.CreateDirectory(Path.Combine(testProjectDirectory.FullName, string.Format(context.OutputPath, index)));
            }
        }

        private static void AddSourceReference(CppTestContext testContext)
        {
            var fileLines = new List<string>();
            var testCodeFile = testContext.TestClass.FullName;

            var start = "<";
            var end = ">";
            var extension = Path.GetExtension(testContext.SourceClass.FullName);
            if (extension.IsHeader())
            {
                end = start = "\"";
            }

            using (var reader = new StreamReader(testCodeFile))
            {
                string line;
                var sourceReferenceAdded = false;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("#") ||
                        line.Trim().StartsWith("//") ||
                        string.IsNullOrWhiteSpace(line) ||
                        sourceReferenceAdded)
                    {
                        fileLines.Add(line);
                        continue;
                    }

                    fileLines.Add($"{Environment.NewLine}#include {start}{testContext.SourceClass.FullName}{end}{Environment.NewLine}");
                    fileLines.Add(line);
                    sourceReferenceAdded = true;
                }
            }

            testCodeFile.WriteLines(fileLines);
        }

        private static void AddNameSpaceWithSourceReference(string codeFile, CppTestContext testContext, int index)
        {
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

                    fileLines.Add($"{Environment.NewLine}#include <{testContext.SourceClass.FullName}>{Environment.NewLine}");
                    fileLines.Add($"namespace mutest_test_{index} {{ {Environment.NewLine}{Environment.NewLine}");
                    fileLines.Add(line);
                    namespaceAdded = true;
                }

                fileLines.Add("}");
            }

            codeFile.WriteLines(fileLines);
        }

        private static void Reset()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
