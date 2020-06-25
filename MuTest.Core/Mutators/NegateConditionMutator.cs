using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class NegateConditionMutator : Mutator<ExpressionSyntax>, IMutator
    {
        public override IEnumerable<Mutation> ApplyMutations(ExpressionSyntax node)
        {
            SyntaxNode replacement = null;
            if (node is IsPatternExpressionSyntax)
            {
                yield break;
            }

            switch (node.Parent)
            {
                case IfStatementSyntax ifStatementSyntax:
                    replacement = NegateCondition(ifStatementSyntax.Condition);
                    break;
                case WhileStatementSyntax whileStatementSyntax:
                    replacement = NegateCondition(whileStatementSyntax.Condition);
                    break;
                case ConditionalExpressionSyntax conditionalExpressionSyntax:
                    if (conditionalExpressionSyntax.Condition == node)
                    {
                        replacement = NegateCondition(conditionalExpressionSyntax.Condition);
                    }
                    break;
                default:
                    yield break;
            }

            if (replacement != null)
            {
                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacement,
                    DisplayName = $"Negating node {node} with {replacement}",
                    Type = MutatorType.Negate
                };
            }
        }

        private static PrefixUnaryExpressionSyntax NegateCondition(ExpressionSyntax expressionSyntax)
        {
            return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, SyntaxFactory.ParenthesizedExpression(expressionSyntax));
        }

        public string Description { get; } = "NEGATE (!Is, Is...)";

        public bool DefaultMutant { get; } = false;
    }
}