using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.AridNodes.Filters;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes
{
    public class AridNodeChecker
    {
        private readonly IAridNodeFilter[] _filters;

        public AridNodeChecker(IAridNodeFilterProvider aridNodeFilterProvider)
        {
            if (aridNodeFilterProvider == null)
            {
                throw new ArgumentNullException(nameof(aridNodeFilterProvider));
            }
            _filters = aridNodeFilterProvider.Filters;
        }

        public AridCheckResult Check(SyntaxNode syntaxNode)
        {
            var node = GetNode(syntaxNode);
            switch (node)
            {
                case CompoundNode compoundNode:
                    return CheckCompoundNode(compoundNode);
                case SimpleNode simpleNode:
                    return CheckSimpleNode(simpleNode);
                default:
                    throw new ArgumentOutOfRangeException(nameof(node));
            }
        }

        private AridCheckResult CheckCompoundNode(CompoundNode compoundNode)
        {
            var filtersTriggered = new List<IAridNodeFilter>();
            foreach (var childNode in compoundNode.SyntaxNode.ChildNodes())
            {
                var check = Check(childNode);
                if (!check.IsArid)
                {
                    return check;
                }
                filtersTriggered.AddRange(check.TriggeredBy);
            }

            return AridCheckResult.CreateForArid(filtersTriggered.ToArray());
        }

        private AridCheckResult CheckSimpleNode(SimpleNode simpleNode)
        {
            var filterPassing = _filters.FirstOrDefault(filter => filter.IsSatisfied(simpleNode));
            return filterPassing != null
                ? AridCheckResult.CreateForArid(filterPassing)
                : AridCheckResult.CreateForNonArid();
        }

        private INode GetNode(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case BlockSyntax _:
                case CheckedStatementSyntax _:
                case ForStatementSyntax _:
                case ForEachVariableStatementSyntax _:
                case CommonForEachStatementSyntax _:
                case DoStatementSyntax _:
                case EmptyStatementSyntax _:
                case IfStatementSyntax _:
                case LabeledStatementSyntax _:
                case LocalFunctionStatementSyntax _:
                case LockStatementSyntax _:
                case SwitchStatementSyntax _:
                case TryStatementSyntax _:
                case UnsafeStatementSyntax _:
                case UsingStatementSyntax _:
                case WhileStatementSyntax _:
                case ExpressionStatementSyntax _:
                    return new CompoundNode(syntaxNode);
                default:
                    return new SimpleNode(syntaxNode);

            }
        }
    }
}