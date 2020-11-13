using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MuTest.Cpp.CodeGenerator.Common;
using IOFile = System.IO.File;
using static MuTest.Cpp.CodeGenerator.Common.Constants;

namespace MuTest.Cpp.CodeGenerator
{
    class Program
    {
        static void Main()
        {
            Directory.SetCurrentDirectory(new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent?.FullName ?? throw new InvalidOperationException());
            while (true)
            {
                Console.Write(EnterSolutionPath);
                var solutionFile = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(solutionFile) ||
                    !IOFile.Exists(solutionFile) ||
                    !Path.GetExtension(solutionFile)
                        .Equals(SolutionExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine(InvalidSolutionFileErrorMessage);
                    continue;
                }

                var solution = new FileInfo(solutionFile);
                var solutionDir = solution.Directory?.FullName;
                var vsDirectory = new DirectoryInfo(
                    Path.Combine(
                        solutionDir,
                        ".vs",
                        Path.GetFileNameWithoutExtension(solution.Name)));

                if (!vsDirectory.Exists)
                {
                    Console.WriteLine(".vs directory does not exist. Please build solution in VS");
                    continue;
                }

                var db = vsDirectory.GetFiles("Browse.VC.db", SearchOption.AllDirectories).FirstOrDefault();
                if (db == null)
                {
                    Console.WriteLine("Browse.VC.db does not exist. Please build solution in VS");
                    continue;
                }

                Console.Write("Enter Source Class Path to Test: ");
                var sourceClass = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(sourceClass) || !IOFile.Exists(sourceClass))
                {
                    Console.WriteLine($"{sourceClass} ... Invalid source code file");
                    continue;
                }

                var context = new BrowseVCContext($"{db.FullName}");
                var source = new FileInfo(sourceClass);

                // Using ToUpper because StringComparison enum is not supported by Entity framework
                var file = context
                    .Files
                    .FirstOrDefault(x => x.Name.Equals(source.FullName.ToUpper()));
                if (file == null)
                {
                    Console.WriteLine($"{sourceClass} ... source code file not testable");
                    break;
                }

                var kinds = context.CodeItemKinds.ToList();
                var codeItems = context.CodeItems.Where(x => x.FileId == file.Id).ToList();

                var functionKind = kinds.First(x => x.Name == "function").Id;
                IncludeKind = kinds.First(x => x.Name == "pound_include").Id;
                var parameterKind = kinds.First(x => x.Name == "parameter").Id;
                var functions = codeItems.Where(x => x.Kind == functionKind).ToList();
                var nonStatic = functions.Any(x => x.Kind == functionKind && x.ParentId != 0);

                var class_name = string.Empty;
                var template = "Templates\\static_class.txt";
                var methodTemplate = "Templates\\static_method.txt";

                if (nonStatic)
                {
                    var classUnderTest = codeItems.First(x => x.Id == functions.First().ParentId);
                    class_name = classUnderTest.Name;
                    template = "Templates\\non_static_class.txt";
                    methodTemplate = "Templates\\non_static_method.txt";
                }

                var outputDir = new DirectoryInfo("Output");
                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }

                outputDir = outputDir.CreateSubdirectory($"{Path.GetFileNameWithoutExtension(source.Name)}Tests_{DateTime.Now:yyyyMdhhmmss}");

                Console.WriteLine("Resolving Imports");

                var imports = new List<string>();
                var rootIncludes = codeItems.Where(x => x.Kind == IncludeKind).ToList();

                foreach (var import in rootIncludes)
                {
                    if (import.Attributes == 262146)
                    {
                        imports.Add($"#include <{import.Name}>");
                        continue;
                    }

                    imports.Add($"#include \"{import.Name}\"");

                    var fileId = context.FileMaps.First(x => x.CodeItemId == import.Id).FileId;

                    CopyNestedIncludes(context, fileId, outputDir);
                }


                var class_under_test_name = Path.GetFileNameWithoutExtension(source.Name);
                var methods = new List<string>();

                var methodTempLines = IOFile.ReadAllLines(methodTemplate);
                foreach (var function in functions)
                {
                    var method_name = function.Name;
                    var parameterList = new List<string>();
                    var argumentList = new List<string>();

                    var type = function.Type.NormalizeName();
                    if (string.IsNullOrWhiteSpace(type))
                    {
                        continue;
                    }

                    var functionParameters =
                        context.CodeItems.Where(x => x.ParentId == function.Id && x.Kind == parameterKind);

                    foreach (var functionParameter in functionParameters)
                    {
                        parameterList.Add($"	{functionParameter.Type.NormalizeName()} {functionParameter.Name} = {{}};");
                        argumentList.Add(functionParameter.Name);
                    }

                    var arguments = string.Join(',', argumentList);
                    var methodBody = new List<string>();

                    foreach (var line in methodTempLines)
                    {
                        if (line.Contains("#class_under_test_name#"))
                        {
                            methodBody.Add(line
                                .Replace("#class_under_test_name#", class_under_test_name)
                                .Replace("#method_name#", method_name));

                            continue;
                        }

                        if (line.Contains("#parameters#"))
                        {
                            foreach (var param in parameterList)
                            {
                                methodBody.Add(param);
                            }

                            continue;
                        }

                        if (line.Contains("#method_name#"))
                        {
                            methodBody.Add(line
                                .Replace("#method_name#", method_name)
                                .Replace("#arguments#", arguments));

                            continue;
                        }

                        methodBody.Add(line);
                    }

                    methods.Add(string.Join('\n', methodBody));
                }

                var testFile = new List<string>();
                foreach (var line in IOFile.ReadAllLines(template))
                {
                    if (line.Contains("#imports#"))
                    {
                        foreach (var import in imports)
                        {
                            testFile.Add(import);
                        }

                        continue;
                    }

                    if (line.Contains("#global_stubs#"))
                    {
                        testFile.Add(line.Replace("#global_stubs#", string.Empty));
                        continue;
                    }

                    if (line.Contains("#class_under_test_path#"))
                    {
                        testFile.Add(line.Replace("#class_under_test_path#", source.FullName));
                        continue;
                    }

                    if (line.Contains("#class_under_test_name#"))
                    {
                        testFile.Add(line.Replace("#class_under_test_name#", class_under_test_name));
                        continue;
                    }

                    if (line.Contains("#class_name#"))
                    {
                        testFile.Add(line.Replace("#class_name#", class_name));
                        continue;
                    }

                    if (line.Contains("#methods#"))
                    {
                        foreach (var method in methods)
                        {
                            testFile.Add(method);
                        }

                        continue;
                    }

                    testFile.Add(line);
                }

                var outputFile = Path.Combine(outputDir.FullName, $"{Path.GetFileNameWithoutExtension(source.Name)}Tests.cpp");
                outputFile.WriteLines(testFile);

                Console.WriteLine("Generated Test Code!");
                Console.WriteLine(outputDir.FullName);
                break;
            }
        }

        private static long IncludeKind { get; set; }

        private static void CopyNestedIncludes(BrowseVCContext context, long fileId, DirectoryInfo outputDir)
        {
            var file = context.Files.FirstOrDefault(x => x.Id == fileId);

            if (file != null)
            {
                var filePath = new FileInfo(file.Name);
                var lines = IOFile.ReadLines(filePath.FullName).ToList();

                var destFileName = Path.Combine(outputDir.FullName, filePath.Name.ToLower());
                if (lines.Any(x => x.Contains("#ifndef")))
                {
                    filePath.CopyTo(destFileName, true);
                }
                else
                {
                    var headerGuardId = $"{DateTime.Now:yyyyMdhhmmss}";
                    var writeLines = new List<string>
                    {
                        $"#ifndef {Path.GetFileNameWithoutExtension(file.LeafName)}_{headerGuardId}_H",
                        $"#define {Path.GetFileNameWithoutExtension(file.LeafName)}_{headerGuardId}_H",
                        string.Empty
                    };

                    writeLines.AddRange(lines);
                    writeLines.Add(string.Empty);
                    writeLines.Add("#endif");

                    destFileName.WriteLines(writeLines);
                }

                var sourceIncludes = context.CodeItems.Where(x =>
                    x.Kind == IncludeKind &&
                    x.FileId == fileId &&
                    x.Attributes != 262146).ToList();
                foreach (var import in sourceIncludes)
                {
                    var nestedFileId = context.FileMaps.First(x => x.CodeItemId == import.Id).FileId;
                    CopyNestedIncludes(context, nestedFileId, outputDir);
                }
            }
        }
    }
}
