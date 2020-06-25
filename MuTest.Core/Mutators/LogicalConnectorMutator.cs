using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class LogicalConnectorMutator : Mutator<BinaryExpressionSyntax>, IMutator
    {
        private IReadOnlyDictionary<SyntaxKind, IEnumerable<SyntaxKind>> KindsToMutate { get; }

        public string Description { get; } = "LOGICAL [&&, ||]";

        public bool DefaultMutant { get; } = true;

        public LogicalConnectorMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, IEnumerable<SyntaxKind>>
            {
                [SyntaxKind.LogicalAndExpression] = new List<SyntaxKind> { SyntaxKind.LogicalOrExpression },
                [SyntaxKind.LogicalOrExpression] = new List<SyntaxKind> { SyntaxKind.LogicalAndExpression }
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
                        DisplayName = $"Logical connector mutation - {node} replace with {replacementNode}",
                        Type = MutatorType.Logical
                    };
                }
            }
        }
    }
}