using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class BitwiseOperatorMutator : Mutator<BinaryExpressionSyntax>, IMutator
    {
        private IReadOnlyDictionary<SyntaxKind, IEnumerable<SyntaxKind>> KindsToMutate { get; }

        public string Description { get; } = "BITWISE [<<, >>, |, &]";

        public bool DefaultMutant { get; } = false;

        public BitwiseOperatorMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, IEnumerable<SyntaxKind>>
            {
                [SyntaxKind.LeftShiftExpression] = new List<SyntaxKind> { SyntaxKind.RightShiftExpression },
                [SyntaxKind.RightShiftExpression] = new List<SyntaxKind> { SyntaxKind.LeftShiftExpression },
                [SyntaxKind.BitwiseOrExpression] = new List<SyntaxKind> { SyntaxKind.BitwiseAndExpression },
                [SyntaxKind.BitwiseAndExpression] = new List<SyntaxKind> { SyntaxKind.BitwiseOrExpression }
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
                            DisplayName = $"Bitwise operator mutation - {node} replace with {replacementNode}",
                            Type = MutatorType.Bitwise
                        };
                }
            }
            else if (node.Kind() == SyntaxKind.ExclusiveOrExpression)
            {
                yield return GetLogicalMutation(node);
                yield return GetIntegralMutation(node);
            }
        }

        private static Mutation GetLogicalMutation(BinaryExpressionSyntax node)
        {
            var replacementNode = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, node.Left, node.Right);
            replacementNode = replacementNode.WithOperatorToken(replacementNode.OperatorToken.WithTriviaFrom(node.OperatorToken));

            return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName = $"Bitwise operator mutation - {node} replace with {replacementNode}",
                Type = MutatorType.Bitwise
            };
        }

        private static Mutation GetIntegralMutation(ExpressionSyntax node)
        {
            var replacementNode = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, SyntaxFactory.ParenthesizedExpression(node));

            return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName = $"Bitwise operator mutation - {node} replace with {replacementNode}",
                Type = MutatorType.Bitwise
            };
        }
    }
}