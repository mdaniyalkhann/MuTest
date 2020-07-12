using MuTest.Core.Model;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes.Filters.ByDefinition
{
    public class ByDefinitionFilter : IAridNodeFilter
    {
        public bool IsSatisfied(IAnalyzableNode node)
        {
            switch (node.SyntaxType)
            {
                case AnalyzableNodeSyntaxType.BinaryExpressionSyntax:
                case AnalyzableNodeSyntaxType.AssignmentExpressionSyntax:
                case AnalyzableNodeSyntaxType.LiteralExpressionSyntax:
                case AnalyzableNodeSyntaxType.CheckedExpressionSyntax:
                case AnalyzableNodeSyntaxType.InterpolatedStringExpressionSyntax:
                case AnalyzableNodeSyntaxType.InvocationExpressionSyntax:
                case AnalyzableNodeSyntaxType.ConditionalExpressionSyntax:
                case AnalyzableNodeSyntaxType.PostfixUnaryExpressionSyntax:
                case AnalyzableNodeSyntaxType.PrefixUnaryExpressionSyntax:
                case AnalyzableNodeSyntaxType.MemberAccessExpressionSyntax:
                case AnalyzableNodeSyntaxType.BlockSyntax:
                case AnalyzableNodeSyntaxType.StatementSyntax:
                    return false;
                default:
                    return true;
            }
        }
    }
}
