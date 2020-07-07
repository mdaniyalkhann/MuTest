using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes
{
    public class ClassNodesClassifier
    {
        private static readonly IAridNodeFilterProvider AridNodeFilterProvider = new AridNodeFilterProvider();
        private static readonly AridNodeChecker AridNodeChecker = new AridNodeChecker(AridNodeFilterProvider);

        public ClassNodesClassification Classify(ClassDeclarationSyntax classDeclarationSyntax)
        {
            if (classDeclarationSyntax == null)
            {
                throw new ArgumentNullException(nameof(classDeclarationSyntax));
            }
            var classNodes = classDeclarationSyntax.DescendantNodes();
            var results = new Dictionary<SyntaxNode, AridCheckResult>();
            foreach (var node in classNodes)
            {
                results[node] = IsAnyParentArid(results, node, out var parentResult)
                    ? AridCheckResult.CreateForArid(parentResult.TriggeredBy)
                    : AridNodeChecker.Check(node);
            }

            return new ClassNodesClassification(results);
        }

        private static bool IsAnyParentArid(IDictionary<SyntaxNode, AridCheckResult> results, SyntaxNode syntaxNode, out AridCheckResult parentResult)
        {
            if (syntaxNode == null)
            {
                throw new ArgumentNullException(nameof(syntaxNode));
            }
            var parent = syntaxNode.Parent;
            while (parent != null && !(parent is StatementSyntax) && !(parent is MethodDeclarationSyntax))
            {
                if (TryGetResult(results, parent, out var result) && result.IsArid)
                {
                    parentResult = result;
                    return true;
                }
                parent = parent.Parent;
            }

            parentResult = null;
            return false;

        }

        private static bool TryGetResult(IDictionary<SyntaxNode, AridCheckResult> results, SyntaxNode syntaxNode, out AridCheckResult result)
        {
            if (!results.ContainsKey(syntaxNode))
            {
                result = null;
                return false;
            }

            result = results[syntaxNode];
            return true;
        }
    }
}