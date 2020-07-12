using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model.ClassDeclarations;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.ClassDeclarationLoaders
{
    public class SemanticsClassDeclarationLoader
    {
        private static readonly CodeAnalysisProjectLoader ProjectLoader = new CodeAnalysisProjectLoader();
        public ClassDeclarationWithSemantics Load(string sourceFilePath, string projectFilePath, string className)
        {
            var allClasses = Load(sourceFilePath, projectFilePath);
            if (string.IsNullOrEmpty(className))
            {
                return allClasses.FirstOrDefault();
            }
            return allClasses.First(c => c.Syntax.ClassName() == className || c.Syntax.FullName() == className);
        }

        public IEnumerable<ClassDeclarationWithSemantics> Load(string sourceFilePath, string projectFilePath)
        {
            var project = ProjectLoader.Load(projectFilePath);
            var document = project.Documents.FirstOrDefault(d => DocumentPathMatches(sourceFilePath, d));
            if (document == null)
            {
                throw new InvalidOperationException(
                    $"Could not locate source file: {sourceFilePath} in project {projectFilePath}");
            }
            var syntaxRoot = document.GetSyntaxRootAsync().Result;
            var semanticModel = document.GetSemanticModelAsync().Result;
            var classDeclarationSyntaxes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>();
            return classDeclarationSyntaxes.Select(c => new ClassDeclarationWithSemantics(c, semanticModel));
        }

        private static bool DocumentPathMatches(string sourceFilePath, TextDocument document)
        {
            return string.Equals(document.FilePath, sourceFilePath,
                StringComparison.InvariantCultureIgnoreCase);
        }
    }
}