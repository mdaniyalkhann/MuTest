using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MuTest.Core.AridNodes;
using MuTest.Core.Common.ClassDeclarationLoaders;
using MuTest.Core.Model;
using MuTest.Core.Model.AridNodes;
using MuTest.Core.Testing;

namespace Mutest.AridNodeClassAnalyzer
{
    internal class Program
    {
        private static readonly NodesClassifier NodesClassifier = new NodesClassifier();
        private static readonly Chalk Chalk = new Chalk();
        
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine($"Usage: {GetExecutableName()} [fullClassFilePath] [fullProjectFilePath]");
                return ExitCodes.Error;
            }
            var classFilePath = args[0];
            var projectFilePath = args[1];
            if (!File.Exists(classFilePath))
            {
                Console.Error.WriteLine($"{classFilePath} does not exist.");
                return ExitCodes.Error;
            }

            if (!File.Exists(projectFilePath))
            {
                Console.Error.WriteLine($"{projectFilePath} does not exist.");
                return ExitCodes.Error;
            }
            DoClassification(classFilePath, projectFilePath, out var htmlReportPath);
            Chalk.Green($"Analysis is complete. Please find your report on {htmlReportPath}");
            return ExitCodes.Success;
        }

        private static void DoClassification(string classFilePath, string projectFilePath, out string htmlReportPath)
        {
            var html = string.Empty;
            var chalk = new ChalkHtml();
            chalk.OutputDataReceived += (sender, output) => html += output;
            var classes = GetClassesFromFile(classFilePath, projectFilePath);
            foreach (var @class in classes)
            {
                var classification = NodesClassifier.Classify(@class);
                WriteClassificationToConsole(@class, classification, chalk);
            }

            html = $@"<html><body style=""background-color: transparent;"">{html}</body></html>";
            htmlReportPath = GetHtmlReportPath();
            var file = new FileInfo(htmlReportPath);
            file.Directory?.Create();
            File.WriteAllText(htmlReportPath, html);
        }

        private static string GetHtmlReportPath()
        {
            var tempFolderPath = Path.Combine(Path.GetTempPath(), "AridNodeReports");
            var htmlReportPath = Path.Combine(tempFolderPath, Guid.NewGuid() + ".html");
            return htmlReportPath;
        }

        private static void WriteClassificationToConsole(
            IAnalyzableNode @class,
            NodesClassification classification,
            IChalk chalk)
        {
            foreach (var node in @class.DescendantNodesAndSelf())
            {
                var result = classification.GetResult(node);
                if (result.IsArid)
                {
                    chalk.Magenta($"ARID {node.Kind()}");
                }
                else
                {
                    chalk.Green($"NON-ARID {node.Kind()}");
                }

                chalk.Default(node.GetText());
                chalk.Default(Environment.NewLine + Environment.NewLine);
            }
        }

        private static string GetExecutableName()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var name = Path.GetFileName(codeBase);
            return name;
        }

        private static IEnumerable<IAnalyzableNode> GetClassesFromFile(string sampleClassAbsolutePath, string sampleProjectAbsolutePath)
        {
            var semanticsClassDeclarationLoader = new SemanticsClassDeclarationLoader();
            var classDeclarations = semanticsClassDeclarationLoader.Load(sampleClassAbsolutePath, sampleProjectAbsolutePath);
            return classDeclarations.Select(c => new RoslynSyntaxNodeWithSemantics(c.Syntax, c.SemanticModel));
        }
    }
}
