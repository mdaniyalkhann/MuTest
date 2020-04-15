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

        public IList<CppTestContext> PrepareTestDirectories(string testClass, string sourceClass, string testProject, string testSolution)
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
            var cppTestContextList = new List<CppTestContext>();

            var projectDirectory = Path.GetDirectoryName(testProject);

            var sourceClassName = Path.GetFileNameWithoutExtension(sourceClass);
            var sourceClassExtension = Path.GetExtension(sourceClass);

            var testClassName = Path.GetFileNameWithoutExtension(testClass);
            var testClassExtension = Path.GetExtension(testClass);

            var testProjectName = Path.GetFileNameWithoutExtension(testProject);
            var testProjectExtension = Path.GetExtension(testProject);

            var testSolutionName = Path.GetFileNameWithoutExtension(testSolution);
            var testSolutionExtension = Path.GetExtension(testSolution);

            var solution = testSolution.GetCodeFileContent();
            var test = testClass.GetCodeFileContent();
            var project = testProject.GetCodeFileContent();

            for (var index = 0; index < NumberOfMutantsExecutingInParallel; index++)
            {
                try
                {
                    var testContext = new CppTestContext
                    {
                        Index = index,
                        IntDir = new FileInfo($"{projectDirectory}\\mutest_int_dir_{index}\\"),
                        OutDir = new FileInfo($"{projectDirectory}\\mutest_out_dir_{index}\\"),
                        IntermediateOutputPath = new FileInfo($"{projectDirectory}\\mutest_obj_dir_{index}\\"),
                        OutputPath = new FileInfo($"{projectDirectory}\\mutest_bin_dir_{index}\\")
                    };

                    var newSourceClass = $"{sourceClassName}_mutest_src_{index}{sourceClassExtension}";
                    var newTestClass = $"{testClassName}_mutest_test_{index}{testClassExtension}";
                    var newTestProject = $"{testProjectName}_mutest_project_{index}{testProjectExtension}";
                    var newTestSolution = $"{testSolutionName}_mutest_sln_{index}{testSolutionExtension}";

                    var testCode = test.Replace(
                        $"{sourceClassName}{sourceClassExtension}",
                        newSourceClass);

                    var solutionCode = solution.Replace($"{testProjectName}{testProjectExtension}", newTestProject);

                    var newSourceClassLocation = $"{Path.GetDirectoryName(sourceClass)}\\{newSourceClass}";
                    var newTestClassLocation = $"{Path.GetDirectoryName(testClass)}\\{newTestClass}";
                    var newTestProjectLocation = $"{projectDirectory}\\{newTestProject}";
                    var newSolutionLocation = $"{Path.GetDirectoryName(testSolution)}\\{newTestSolution}";

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

                    var relativeTestCodePath = testClass.RelativePath(projectDirectory);
                    var relativeNewTestCodePath = newTestClassLocation.RelativePath(projectDirectory);
                    var testProjectXml = project
                        .Replace(relativeTestCodePath, relativeNewTestCodePath);

                    testProjectXml.UpdateCode(newTestProjectLocation);
                    testContext.TestProject = new FileInfo(newTestProjectLocation);

                    solutionCode.UpdateCode(newSolutionLocation);
                    testContext.TestSolution = new FileInfo(newSolutionLocation);

                    cppTestContextList.Add(testContext);
                }
                catch (Exception exp)
                {
                    Console.WriteLine($"Unable to prepare Cpp Test Directories: {exp.Message}");
                    Trace.TraceError(exp.ToString());
                    cppTestContextList.Clear();
                }
            }

            return cppTestContextList;
        }

        private static void Reset()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
