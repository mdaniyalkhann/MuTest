using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MuTest.Core.Utility;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Utility;

namespace MuTest.Cpp.CLI.Core
{
    internal class CppDirectoryFactory
    {
        public int NumberOfMutantsExecutingInParallel { get; set; } = 5;

        public CppBuildContext PrepareTestDirectories(string testClass, string sourceClass, string testProject, string testSolution)
        {
            if (testClass == null)
            {
                throw new ArgumentNullException(nameof(testClass));
            }

            if (sourceClass == null)
            {
                throw new ArgumentNullException(nameof(sourceClass));
            }

            if (testProject == null)
            {
                throw new ArgumentNullException(nameof(testProject));
            }

            if (testSolution == null)
            {
                throw new ArgumentNullException(nameof(testSolution));
            }

            Reset();

            var projectDirectory = Path.GetDirectoryName(testProject);

            var testProjectName = Path.GetFileNameWithoutExtension(testProject);
            var testProjectExtension = Path.GetExtension(testProject);

            var testSolutionName = Path.GetFileNameWithoutExtension(testSolution);
            var testSolutionExtension = Path.GetExtension(testSolution);

            var solution = testSolution.GetCodeFileContent();
            var test = testClass.GetCodeFileContent();

            var newTestProject = $"{testProjectName}_mutest_project{testProjectExtension}";
            var newTestSolution = $"{testSolutionName}_mutest_sln{testSolutionExtension}";

            var newTestProjectLocation = $"{projectDirectory}\\{newTestProject}";
            var newSolutionLocation = $"{Path.GetDirectoryName(testSolution)}\\{newTestSolution}";

            var solutionCode = solution.Replace($"{testProjectName}{testProjectExtension}", newTestProject);
            solutionCode.UpdateCode(newSolutionLocation);

            new FileInfo(testProject).CopyTo(newTestProjectLocation, true);

            var context = new CppBuildContext
            {
                IntDir = "mutest_int_dir/",
                OutDir = "mutest_out_dir/",
                IntermediateOutputPath = "mutest_obj_dir/",
                OutputPath = "mutest_bin_dir/",
                TestProject = new FileInfo(newTestProjectLocation),
                TestSolution = new FileInfo(newSolutionLocation)
            };

            var sourceClassName = Path.GetFileNameWithoutExtension(sourceClass);
            var sourceClassExtension = Path.GetExtension(sourceClass);

            var testClassName = Path.GetFileNameWithoutExtension(testClass);
            var testClassExtension = Path.GetExtension(testClass);

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                try
                {
                    var testContext = new CppTestContext
                    {
                        Index = index
                   
                    };

                    var newSourceClass = $"{sourceClassName}_mutest_src_{index}{sourceClassExtension}";
                    var newTestClass = $"{testClassName}_mutest_test_{index}{testClassExtension}";

                    var testCode = test.Replace(
                        $"{sourceClassName}{sourceClassExtension}",
                        newSourceClass)
                        .Replace(testClassName, Path.GetFileNameWithoutExtension(newTestClass));

                    var newSourceClassLocation = $"{Path.GetDirectoryName(sourceClass)}\\{newSourceClass}";
                    var newTestClassLocation = $"{Path.GetDirectoryName(testClass)}\\{newTestClass}";

                    if (File.Exists(newSourceClassLocation))
                    {
                        File.Delete(newSourceClassLocation);
                    }

                    if (File.Exists(newTestClassLocation))
                    {
                        File.Delete(newTestClassLocation);
                    }

                    new FileInfo(sourceClass).CopyTo(newSourceClassLocation, true);
                    testContext.SourceClass = new FileInfo(newSourceClassLocation);

                    testCode.UpdateCode(newTestClassLocation);
                    testContext.TestClass = new FileInfo(newTestClassLocation);

                    var fileLines = new List<string>();
                    using (var reader = new StreamReader(newTestClassLocation))
                    {
                        string line;
                        var namespaceAdded = false;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Trim().StartsWith("#include") || string.IsNullOrWhiteSpace(line) || namespaceAdded)
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

                    newTestClassLocation.WriteLines(fileLines);

                    var relativeTestCodePath = testClass.RelativePath(projectDirectory);
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

        private static void Reset()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
