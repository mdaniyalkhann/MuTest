using System;
using System.Collections.Generic;
using System.Linq;
using MuTest.Core.AridNodes.Filters;
using MuTest.Core.Model;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes
{
    public class NodesClassifier
    {
        private static readonly IAridNodeFilterProvider AridNodeFilterProvider = new AridNodeFilterProvider();
        private readonly IAridNodeFilter[] _filters;

        public NodesClassifier()
        {
            _filters = AridNodeFilterProvider.Filters;
        }

        public NodesClassification Classify(IAnalyzableNode root)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }
            var syntaxNodes = root.DescendantNodesAndSelf();
            var results = new Dictionary<IAnalyzableNode, AridCheckResult>();
            foreach (var node in syntaxNodes)
            {
                results[node] = Check(node, results);
            }

            return new NodesClassification(results);
        }

        public AridCheckResult Check(IAnalyzableNode node, IDictionary<IAnalyzableNode, AridCheckResult> results)
        {
            if (IsAnyParentArid(results, node, out var parentResult))
            {
                return AridCheckResult.CreateForArid(parentResult.TriggeredBy);
            }
            return node.IsCompoundNode ? CheckCompoundNode(node, results) : CheckSimpleNode(node);
        }

        private AridCheckResult CheckCompoundNode(IAnalyzableNode node, IDictionary<IAnalyzableNode, AridCheckResult> results)
        {
            var filtersTriggered = new List<IAridNodeFilter>();
            var childNodes = node.ChildNodes();

            foreach (var childNode in childNodes)
            {
                var check = Check(childNode, results);
                if (!check.IsArid)
                {
                    return check;
                }
                filtersTriggered.AddRange(check.TriggeredBy);
            }

            return AridCheckResult.CreateForArid(filtersTriggered.ToArray());
        }

        private AridCheckResult CheckSimpleNode(IAnalyzableNode node)
        {
            var filterPassing = _filters.FirstOrDefault(filter => filter.IsSatisfied(node));
            return filterPassing != null
                ? AridCheckResult.CreateForArid(filterPassing)
                : AridCheckResult.CreateForNonArid();
        }

        private static bool IsAnyParentArid(IDictionary<IAnalyzableNode, AridCheckResult> results, IAnalyzableNode node, out AridCheckResult parentResult)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var parent = node.Parent;
            while (parent != null && 
                   parent.SyntaxType != AnalyzableNodeSyntaxType.StatementSyntax &&
                   parent.SyntaxType != AnalyzableNodeSyntaxType.MethodDeclarationSyntax && 
                   parent.SyntaxType != AnalyzableNodeSyntaxType.PropertyDeclarationSyntax)
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

        private static bool TryGetResult(IDictionary<IAnalyzableNode, AridCheckResult> results, IAnalyzableNode syntaxNode, out AridCheckResult result)
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