using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using MuTest.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MuTest.CoverageAnalyzer
{
    internal class MsBuildClassExtractor : IClassExtractor
    {
        private static readonly SymbolDisplayFormat FullyQualifiedSymbolDisplayFormat = new SymbolDisplayFormat(
           genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
           typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
           miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

        static MsBuildClassExtractor()
        {
            MSBuildLocator.RegisterDefaults();
        }

        public IEnumerable<Model.Class> ExtractClasses(Model.Project sourceProject)
        {
            var sourceProjectAnalysis = Load(sourceProject.AbsolutePath);
            var documents = sourceProjectAnalysis.Documents;
            var allClasses = new List<Model.Class>();
            foreach (var document in documents)
            {
                var syntaxRoot = document.GetSyntaxRootAsync().Result;
                var semanticModel = document.GetSemanticModelAsync().Result;
                var classes = syntaxRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().Select(c =>
                {
                    var className = semanticModel.GetDeclaredSymbol(c).ToDisplayString(FullyQualifiedSymbolDisplayFormat);
                    var exactFilePath = document.FilePath.GetExactPathName();
                    return new Model.Class
                    {
                        DeclarationSyntax = c,
                        FullClassName = className,
                        FilePath = exactFilePath
                    };
                }).ToList();
                allClasses.AddRange(classes);
            }

            return allClasses;
        }

        private static Project Load(string projectPath)
        {
            var properties = new Dictionary<string, string>()
            {
                ["AutoGenerateBindingRedirects"] = "true",
                ["GenerateBindingRedirectsOutputType"] = "true",
                ["AlwaysCompileMarkupFilesInSeparateDomain"] = "false"  // Due to: https://github.com/dotnet/roslyn/issues/29780
            };
            var workspace = MSBuildWorkspace.Create(properties);
            workspace.LoadMetadataForReferencedProjects = true;
            var project = workspace.OpenProjectAsync(projectPath).Result;

            var compilation = project.GetCompilationAsync().Result;
            var errors = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

            if (!errors.Any())
            {
                var newCompilationOptions =
                    project.CompilationOptions.WithMetadataImportOptions(MetadataImportOptions.All);
                project = project.WithCompilationOptions(newCompilationOptions);
                return project;
            }

            throw new InvalidOperationException(
                $"There were some errors during loading of project located on {projectPath}. Please ensure that the project builds successfully in Visual Studio.");
        }
    }
}
