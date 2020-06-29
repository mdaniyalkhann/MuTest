using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class PostfixUnaryMutator : Mutator<PostfixUnaryExpressionSyntax>, IMutator
    {
        public string Description { get; } = "POSTFIX UNARY [index++, index--]";

        public bool DefaultMutant { get; } = true;

        private static readonly IReadOnlyDictionary<SyntaxKind, SyntaxKind> UnaryWithOpposite = new Dictionary<SyntaxKind, SyntaxKind>
        {
            [SyntaxKind.PostIncrementExpression] = SyntaxKind.PostDecrementExpression,
            [SyntaxKind.PostDecrementExpression] = SyntaxKind.PostIncrementExpression
        };

        public override IEnumerable<Mutation> ApplyMutations(PostfixUnaryExpressionSyntax node)
        {
            var unaryKind = node.Kind();
            if (UnaryWithOpposite.TryGetValue(unaryKind, out var oppositeKind))
            {
                var replacementNode = SyntaxFactory.PostfixUnaryExpression(oppositeKind, node.Operand);
                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"{unaryKind} to {oppositeKind} mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.Unary
                };
            }
        }
    }
}