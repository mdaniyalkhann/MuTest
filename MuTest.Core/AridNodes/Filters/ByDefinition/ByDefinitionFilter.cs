using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes.Filters.ByDefinition
{
    public class ByDefinitionFilter : IAridNodeFilter
    {
        public bool IsSatisfied(SimpleNode node)
        {
            switch (node.SyntaxNode)
            {
                case BinaryExpressionSyntax _:
                case AssignmentExpressionSyntax _:
                case LiteralExpressionSyntax _:
                case CheckedExpressionSyntax _:
                case InterpolatedStringExpressionSyntax _:
                case InvocationExpressionSyntax _:
                case ConditionalExpressionSyntax _:
                case PostfixUnaryExpressionSyntax _:
                case PrefixUnaryExpressionSyntax _:
                case MemberAccessExpressionSyntax _:
                case ArgumentListSyntax _:
                case ArgumentSyntax _:
                case IdentifierNameSyntax _:
                    return false;
                default:
                    return true;
            }
        }
    }
}
