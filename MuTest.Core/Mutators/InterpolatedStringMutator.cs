using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class InterpolatedStringMutator: Mutator<InterpolatedStringExpressionSyntax>, IMutator
    {
        public string Description { get; } = "INTERPOLATED_STRING";

        public bool DefaultMutant { get; } = false;

        public override IEnumerable<Mutation> ApplyMutations(InterpolatedStringExpressionSyntax node)
        {
            if (node.Contents.Any())
            {
                var replacementNode = CreateEmptyInterpolatedString();
                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"String mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.String
                };
            }
        }

        private SyntaxNode CreateEmptyInterpolatedString()
        {
            var opening = SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken);
            var closing = SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken);
            var emptyText = new SyntaxList<InterpolatedStringContentSyntax>
                {
                    SyntaxFactory.InterpolatedStringText()
                };

            return SyntaxFactory.InterpolatedStringExpression(opening, emptyText, closing);
        }
    }
}
