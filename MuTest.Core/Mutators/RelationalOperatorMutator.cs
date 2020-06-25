using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class RelationalOperatorMutator : Mutator<BinaryExpressionSyntax>, IMutator
    {
        private IReadOnlyDictionary<SyntaxKind, IEnumerable<SyntaxKind>> KindsToMutate { get; }

        public string Description { get; } = "RELATIONAL [<, <=, >, >=, ==, !=]";

        public bool DefaultMutant { get; } = true;

        public RelationalOperatorMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, IEnumerable<SyntaxKind>>
            {
                [SyntaxKind.GreaterThanExpression] = new List<SyntaxKind> { SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanOrEqualExpression },
                [SyntaxKind.LessThanExpression] = new List<SyntaxKind> { SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanOrEqualExpression },
                [SyntaxKind.GreaterThanOrEqualExpression] = new List<SyntaxKind> { SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanExpression },
                [SyntaxKind.LessThanOrEqualExpression] = new List<SyntaxKind> { SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanExpression },
                [SyntaxKind.EqualsExpression] = new List<SyntaxKind> { SyntaxKind.NotEqualsExpression },
                [SyntaxKind.NotEqualsExpression] = new List<SyntaxKind> { SyntaxKind.EqualsExpression }
            };
        }

        public override IEnumerable<Mutation> ApplyMutations(BinaryExpressionSyntax node)
        {
            if (KindsToMutate.ContainsKey(node.Kind()))
            {
                foreach (var mutationKind in KindsToMutate[node.Kind()])
                {
                    var nodeLeft = node.Left;
                    var replacementNode = SyntaxFactory.BinaryExpression(mutationKind, nodeLeft, node.Right);
                    replacementNode = replacementNode.WithOperatorToken(replacementNode.OperatorToken.WithTriviaFrom(node.OperatorToken));

                    yield return new Mutation
                    {
                        OriginalNode = node,
                        ReplacementNode = replacementNode,
                        DisplayName = $"Relational operator mutation - {node} replace with {replacementNode}",
                        Type = MutatorType.Relational
                    };
                }
            }
        }
    }
}