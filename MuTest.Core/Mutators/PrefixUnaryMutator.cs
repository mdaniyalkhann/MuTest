using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class PrefixUnaryMutator : Mutator<PrefixUnaryExpressionSyntax>, IMutator
    {
        public string Description { get; } = "PREFIX UNARY [++index, --index]";

        public bool DefaultMutant { get; } = true;

        private static readonly IReadOnlyDictionary<SyntaxKind, SyntaxKind> UnaryWithOpposite = new Dictionary<SyntaxKind, SyntaxKind>
        {
            [SyntaxKind.UnaryMinusExpression] = SyntaxKind.UnaryPlusExpression,
            [SyntaxKind.UnaryPlusExpression] = SyntaxKind.UnaryMinusExpression,
            [SyntaxKind.PreIncrementExpression] = SyntaxKind.PreDecrementExpression,
            [SyntaxKind.PreDecrementExpression] = SyntaxKind.PreIncrementExpression
        };

        private static readonly HashSet<SyntaxKind> UnaryToInitial = new HashSet<SyntaxKind>
        {
            SyntaxKind.BitwiseNotExpression,
            SyntaxKind.LogicalNotExpression
        };

        public override IEnumerable<Mutation> ApplyMutations(PrefixUnaryExpressionSyntax node)
        {
            var unaryKind = node.Kind();
            if (UnaryWithOpposite.TryGetValue(unaryKind, out var oppositeKind))
            {
                var replacementNode = SyntaxFactory.PrefixUnaryExpression(oppositeKind, node.Operand);
                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"{unaryKind} to {oppositeKind} mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.Unary
                };
            }
            else if (UnaryToInitial.Contains(unaryKind))
            {
                var replacementNode = node.Operand;
                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"{unaryKind} to un-{unaryKind} mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.Unary 
                };
            }
        }
    }
}