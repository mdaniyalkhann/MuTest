using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MuTest.Core.Utility;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Utility;

namespace MuTest.Cpp.CLI.Core
{
    public class CppDirectoryFactory : ICppDirectoryFactory
    {
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

            var solutionCode = solution.Replace($"{testProjectName}{testProjectExtension}", newTestProject);
            solutionCode.UpdateCode(newSolutionLocation);

            new FileInfo(cppClass.TestProject).CopyTo(newTestProjectLocation, true);

            var context = new CppBuildContext
            {
                IntDir = "mutest_int_dir/",
                OutDir = "mutest_out_dir/",
                IntermediateOutputPath = "mutest_obj_dir/",
                OutputPath = "mutest_bin_dir/",
                TestProject = new FileInfo(newTestProjectLocation),
                TestSolution = new FileInfo(newSolutionLocation)
            };

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

                    var testCode = test.Replace(
                        $"{sourceClassName}{sourceClassExtension}",
                        newSourceClass)
                        .Replace(testClassName, Path.GetFileNameWithoutExtension(newTestClass))
                        .Replace($"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader);

                    var newSourceClassLocation = $"{Path.GetDirectoryName(cppClass.SourceClass)}\\{newSourceClass}";
                    var newHeaderClassLocation = $"{Path.GetDirectoryName(cppClass.SourceHeader)}\\{newSourceHeader}";
                    var newTestClassLocation = $"{Path.GetDirectoryName(cppClass.TestClass)}\\{newTestClass}";

                    newSourceClassLocation.DeleteIfExists();
                    newTestClassLocation.DeleteIfExists();

                    testContext.SourceClass = new FileInfo(newSourceClassLocation);
                    testContext.SourceHeader = new FileInfo(newHeaderClassLocation);

                    if (!sourceHeaderExtension.Equals(sourceClassExtension))
                    {
                        var sourceCode = source.Replace($"{sourceHeaderName}{sourceHeaderExtension}", newSourceHeader);
                        sourceCode.UpdateCode(newSourceClassLocation);
                        new FileInfo(cppClass.SourceHeader).CopyTo(newHeaderClassLocation, true);

                        newHeaderClassLocation.AddNameSpace(index);
                        newSourceClassLocation.AddNameSpace(index);
                    }
                    else
                    {
                        new FileInfo(cppClass.SourceClass).CopyTo(newSourceClassLocation);
                    }

                    testCode.UpdateCode(newTestClassLocation);
                    testContext.TestClass = new FileInfo(newTestClassLocation);

                    AddNameSpaceWithSourceReference(newTestClassLocation, testContext, index);

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

        public void DeleteTestFiles(CppBuildContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.TestProject.DeleteIfExists();
            context.TestSolution.DeleteIfExists();

            foreach (var testContext in context.TestContexts)
            {
                testContext.SourceClass.DeleteIfExists();
                testContext.SourceHeader.DeleteIfExists();
                testContext.TestClass.DeleteIfExists();
            }
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
