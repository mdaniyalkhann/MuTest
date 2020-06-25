using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class ArithmeticOperatorMutator : Mutator<BinaryExpressionSyntax>, IMutator
    {
        private IReadOnlyDictionary<SyntaxKind, IEnumerable<SyntaxKind>> KindsToMutate { get; }

        public string Description { get; } = "ARITHMETIC [+, - , / , %]";

        public bool DefaultMutant { get; } = true;

        public ArithmeticOperatorMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, IEnumerable<SyntaxKind>>
            {
                [SyntaxKind.SubtractExpression] = new List<SyntaxKind> { SyntaxKind.AddExpression },
                [SyntaxKind.AddExpression] = new List<SyntaxKind> { SyntaxKind.SubtractExpression },
                [SyntaxKind.MultiplyExpression] = new List<SyntaxKind> { SyntaxKind.DivideExpression },
                [SyntaxKind.DivideExpression] = new List<SyntaxKind> { SyntaxKind.MultiplyExpression },
                [SyntaxKind.ModuloExpression] = new List<SyntaxKind> { SyntaxKind.MultiplyExpression }
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
                            DisplayName = $"Arithmetic operator mutation - {node} replace with {nodeLeft}",
                            Type = MutatorType.Arithmetic
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
                            DisplayName = $"Arithmetic operator mutation - {node} replace with {replacementNode}",
                            Type = MutatorType.Arithmetic
                        };
                    }
                }
            }
        }
    }
}