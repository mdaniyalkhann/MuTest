using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.AridNodes;
using MuTest.Core.Model.AridNodes;
using MuTest.Core.Testing;

namespace Mutest.AridNodeClassAnalyzer
{
    internal class Program
    {
        private static readonly ClassNodesClassifier ClassNodesClassifier = new ClassNodesClassifier();
        private static readonly Chalk Chalk = new Chalk();
        
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine($"Usage: {GetExecutableName()} [fullClassFilePath]");
                return ExitCodes.Error;
            }
            var classFilePath = args[0];
            if (!File.Exists(classFilePath))
            {
                Console.Error.WriteLine($"{classFilePath} does not exist.");
                return ExitCodes.Error;
            }
            DoClassification(classFilePath, out var htmlReportPath);
            Chalk.Green($"Analysis is complete. Please find your report on {htmlReportPath}");
            return ExitCodes.Success;
        }

        private static void DoClassification(string classFilePath, out string htmlReportPath)
        {
            var html = string.Empty;
            var chalk = new ChalkHtml();
            chalk.OutputDataReceived += (sender, output) => html += output;
            var classDeclarationSyntaxes = GetClassDeclarationSyntaxes(classFilePath);
            foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
            {
                var classification = ClassNodesClassifier.Classify(classDeclarationSyntax);
                WriteClassificationToConsole(classDeclarationSyntax, classification, chalk);
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
            SyntaxNode classDeclarationSyntax,
            ClassNodesClassification classification,
            IChalk chalk)
        {
            foreach (var syntaxNode in classDeclarationSyntax.DescendantNodes())
            {
                var result = classification.GetResult(syntaxNode);
                if (result.IsArid)
                {
                    chalk.Magenta($"ARID {syntaxNode.Kind().ToString()}");
                }
                else
                {
                    chalk.Green($"NON-ARID {syntaxNode.Kind().ToString()}");
                }

                chalk.Default(syntaxNode.GetText().ToString());
                chalk.Default(Environment.NewLine + Environment.NewLine);
            }
        }

        private static string GetExecutableName()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var name = Path.GetFileName(codeBase);
            return name;
        }

        private static IEnumerable<ClassDeclarationSyntax> GetClassDeclarationSyntaxes(string sampleClassRelativePath)
        {
            var sampleClassText = File.ReadAllText(sampleClassRelativePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(sampleClassText);
            var root = syntaxTree.GetCompilationUnitRoot();
            var classDeclarationSyntaxes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            return classDeclarationSyntaxes;
        }
    }
}
