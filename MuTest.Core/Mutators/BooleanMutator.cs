using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class BooleanMutator : Mutator<LiteralExpressionSyntax>, IMutator
    {
        public string Description { get; } = "BOOLEAN [false, true]";

        private IReadOnlyDictionary<SyntaxKind, SyntaxKind> KindsToMutate { get; }

        public bool DefaultMutant { get; } = false;

        public BooleanMutator()
        {
            KindsToMutate = new Dictionary<SyntaxKind, SyntaxKind>
            {
                [SyntaxKind.TrueLiteralExpression] = SyntaxKind.FalseLiteralExpression,
                [SyntaxKind.FalseLiteralExpression] = SyntaxKind.TrueLiteralExpression
            };
        }

        public override IEnumerable<Mutation> ApplyMutations(LiteralExpressionSyntax node)
        {
            if (KindsToMutate.ContainsKey(node.Kind()))
            {
                var replacementNode = SyntaxFactory.LiteralExpression(KindsToMutate[node.Kind()]);
                yield return new Mutation()
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"Boolean mutation mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.Boolean
                };
            }
        }
    }
}