using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class BinaryExpressionMutator : Mutator<BinaryExpressionSyntax>, IMutator
    {
        private IReadOnlyDictionary<SyntaxKind, IEnumerable<SyntaxKind>> KindsToMutate { get; }

        public string Description { get; } = "BINARY [+, - , / , %, <, <=, >, >=, ==, !=, &&, ||]";

        public bool DefaultMutant { get; } = true;

        public BinaryExpressionMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, IEnumerable<SyntaxKind>>
            {
                [SyntaxKind.SubtractExpression] = new List<SyntaxKind> { SyntaxKind.AddExpression },
                [SyntaxKind.AddExpression] = new List<SyntaxKind> { SyntaxKind.SubtractExpression },
                [SyntaxKind.MultiplyExpression] = new List<SyntaxKind> { SyntaxKind.DivideExpression },
                [SyntaxKind.DivideExpression] = new List<SyntaxKind> { SyntaxKind.MultiplyExpression },
                [SyntaxKind.ModuloExpression] = new List<SyntaxKind> { SyntaxKind.MultiplyExpression },
                [SyntaxKind.GreaterThanExpression] = new List<SyntaxKind> { SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanOrEqualExpression },
                [SyntaxKind.LessThanExpression] = new List<SyntaxKind> { SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanOrEqualExpression },
                [SyntaxKind.GreaterThanOrEqualExpression] = new List<SyntaxKind> { SyntaxKind.LessThanExpression, SyntaxKind.GreaterThanExpression },
                [SyntaxKind.LessThanOrEqualExpression] = new List<SyntaxKind> { SyntaxKind.GreaterThanExpression, SyntaxKind.LessThanExpression },
                [SyntaxKind.EqualsExpression] = new List<SyntaxKind> { SyntaxKind.NotEqualsExpression },
                [SyntaxKind.NotEqualsExpression] = new List<SyntaxKind> { SyntaxKind.EqualsExpression },
                [SyntaxKind.LogicalAndExpression] = new List<SyntaxKind> { SyntaxKind.LogicalOrExpression },
                [SyntaxKind.LogicalOrExpression] = new List<SyntaxKind> { SyntaxKind.LogicalAndExpression },
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
                    if (node.Kind() is SyntaxKind.AddExpression)
                    {
                        yield return new Mutation
                        {
                            OriginalNode = node,
                            ReplacementNode = nodeLeft,
                            DisplayName = $"Binary expression mutation - {node} replace with {nodeLeft}",
                            Type = GetMutatorType(mutationKind)
                        };
                    }
                    else
                    {
                        var replacementNode = SyntaxFactory.BinaryExpression(mutationKind, nodeLeft, node.Right);
                        replacementNode = replacementNode.WithOperatorToken(replacementNode.OperatorToken.WithTriviaFrom(node.OperatorToken));

                        yield return new Mutation
                        {
                            OriginalNode = node,
                            ReplacementNode = replacementNode,
                            DisplayName = $"Binary expression mutation - {node} replace with {replacementNode}",
                            Type = GetMutatorType(mutationKind)
                        };
                    }
                }
            }
            else if (node.Kind() == SyntaxKind.ExclusiveOrExpression)
            {
                yield return GetLogicalMutation(node);
                yield return GetIntegralMutation(node);
            }
        }

        private static MutatorType GetMutatorType(SyntaxKind kind)
        {
            var kindString = kind.ToString();
            if (kindString.StartsWith("Logical"))
            {
                return MutatorType.Logical;
            }

            if (kindString.Contains("Equals")
                || kindString.Contains("Greater")
                || kindString.Contains("Less"))
            {
                return MutatorType.Equality;
            }

            return MutatorType.Arithmetic;
        }

        private static Mutation GetLogicalMutation(BinaryExpressionSyntax node)
        {
            var replacementNode = SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, node.Left, node.Right);
            replacementNode = replacementNode.WithOperatorToken(replacementNode.OperatorToken.WithTriviaFrom(node.OperatorToken));

            return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName = $"Binary expression mutation - {node} replace with {replacementNode}",
                Type = MutatorType.Logical
            };
        }

        private static Mutation GetIntegralMutation(ExpressionSyntax node)
        {
            var replacementNode = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, SyntaxFactory.ParenthesizedExpression(node));

            return new Mutation
            {
                OriginalNode = node,
                ReplacementNode = replacementNode,
                DisplayName = $"Bitwise Binary expression mutation - {node} replace with {replacementNode}",
                Type = MutatorType.Logical
            };
        }
    }
}