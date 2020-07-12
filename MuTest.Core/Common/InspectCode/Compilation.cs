using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode
{
    public static class Compilation
    {
        public static IDictionary<string, SemanticModel> SemanticModels { get; } = new Dictionary<string, SemanticModel>();

        public static IList<NodeLocation> UnusedVariables { get; } = new List<NodeLocation>();

        private static readonly IList<MetadataReference> CommonReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Configuration.AppSettingsReader).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Data.DataColumn).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Xml.XmlAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Web.UI.Page).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XNode).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Windows.Forms.Label).Assembly.Location)
        };

        public static TestClassDetail TestClass { get; set; }

        public static void LoadSemanticModels()
        {
            if (string.IsNullOrWhiteSpace(TestClass?.ClassLibrary))
            {
                throw new InvalidOperationException("Set Class Library Path");
            }

            if (TestClass?.Claz == null)
            {
                throw new InvalidOperationException("Class Syntax Tree is missing");
            }

            var assemblyDir = Directory.GetParent(TestClass.ClassLibrary);
            var allAssembliesFiles = assemblyDir.GetFiles("*.dll", SearchOption.AllDirectories);
            var metaDataList = new List<MetadataReference>(CommonReferences);

            metaDataList
                .AddRange(allAssembliesFiles
                    .Select(file => MetadataReference.CreateFromFile(file.FullName)));

            var trees = TestClass.PartialClasses.Select(c => c.Claz.Syntax.Root().SyntaxTree).ToList();
            CSharpCompilation compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(metaDataList)
                .AddSyntaxTrees(trees);

            foreach (var partialClass in TestClass.PartialClasses)
            {
                var tree = partialClass.Claz.Syntax.Root().SyntaxTree;
                var semanticModel = compilation.GetSemanticModel(tree);
                if (!SemanticModels.ContainsKey(partialClass.FilePath))
                {
                    SemanticModels.Add(partialClass.FilePath, semanticModel);
                }

                LoadUnusedVariables(tree, partialClass, semanticModel);
            }
        }

        private static void LoadUnusedVariables(SyntaxTree tree, ClassDetail partialClass, SemanticModel semanticModel)
        {
            var methods = tree
                .GetRoot()
                .GetMethods();

            var location = CreateLocationNode(partialClass);

            foreach (var method in methods)
            {
                if (method.Body == null)
                {
                    continue;
                }

                var dataFlow = semanticModel.AnalyzeDataFlow(method.Body);
                var variablesDeclared = dataFlow.VariablesDeclared.Where(x => x.Kind == SymbolKind.Local);
                var parametersDeclared = dataFlow.WrittenOutside.Where(x => x.Kind == SymbolKind.Parameter);
                var variablesRead = dataFlow.ReadInside.Union(dataFlow.ReadOutside).ToList();

                var unused = variablesDeclared.Except(variablesRead).ToList();
                unused.AddRange(parametersDeclared.Except(variablesRead));
                if (unused.Any())
                {
                    foreach (var unusedVar in unused)
                    {
                        var foreachStatements = method.DescendantNodes<ForEachStatementSyntax>()
                            .Any(x => x.Identifier.Text == unusedVar.Name);
                        if (foreachStatements)
                        {
                            continue;
                        }

                        location.Locations.Add(unusedVar.Locations.First());
                    }
                }
            }

            UnusedVariables.Add(location);
        }

        private static NodeLocation CreateLocationNode(ClassDetail partialClass)
        {
            return new NodeLocation
            {
                FilePath = partialClass.FilePath
            };
        }

        public static void UnLoadSemanticModels()
        {
            SemanticModels.Clear();
        }
    }
}