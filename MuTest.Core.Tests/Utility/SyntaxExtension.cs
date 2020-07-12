using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Tests.Utility
{
    [ExcludeFromCodeCoverage]
    public static class SyntaxExtension
    {
        public static IEnumerable<SyntaxNode> GetDescendantNodesOfClass(this string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            var sampleClassText = GetSampleClassText(relativePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(sampleClassText);
            var root = syntaxTree.GetCompilationUnitRoot();
            var nodes = root.DescendantNodes();
            return nodes;
        }

        public static ClassDeclarationSyntax GetSampleClassDeclarationSyntax(this string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            return GetDescendantNodesOfClass(relativePath).OfType<ClassDeclarationSyntax>().First();
        }

        public static string GetSampleClassesRootDirectory()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var assemblyFile = new FileInfo(assemblyLocation);
            var root = assemblyFile.Directory?.Parent?.Parent;
            return root?.FullName;
        }

        public static string GetSampleProjectAbsolutePath()
        {
            var sampleProjectRoot = GetSampleClassesRootDirectory();
            return Path.Combine(sampleProjectRoot, "MuTest.Core.Tests.csproj");
        }

        public static Func<ClassDeclarationSyntax, SyntaxNode> GetFirstSyntaxNodeOfMethodFunc<TSyntaxNode>(this string methodName) where TSyntaxNode : SyntaxNode
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            SyntaxNode GetSyntaxNode(ClassDeclarationSyntax classDeclarationSyntax) =>
                classDeclarationSyntax.DescendantNodes()
                    .OfType<MethodDeclarationSyntax>()
                    .First(m => m.Identifier.Text == methodName)
                    .DescendantNodes()
                    .OfType<TSyntaxNode>()
                    .First();

            return GetSyntaxNode;
        }

        public static string GetSampleClassAbsoluteFilePath(this string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            var root = GetSampleClassesRootDirectory();
            return Path.Combine(root, relativePath);
        }

        public static string GetSampleClassText(this string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            var absoluteFilePath = GetSampleClassAbsoluteFilePath(relativePath);
            return File.ReadAllText(absoluteFilePath);
        }
    }
}
