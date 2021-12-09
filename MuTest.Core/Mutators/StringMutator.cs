using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;
using MuTest.Core.Utility;

namespace MuTest.Core.Mutators
{
    public class StringMutator : Mutator<LiteralExpressionSyntax>, IMutator
    {
        public string Description { get; } = "STRING";

        public bool DefaultMutant { get; } = true;

        public override IEnumerable<Mutation> ApplyMutations(LiteralExpressionSyntax node)
        {
            var kind = node.Kind();
            if (kind == SyntaxKind.StringLiteralExpression)
            {
                var currentValue = (string) node.Token.Value;
                var replacementValue = currentValue == string.Empty
                    ? "mutation-test-value"
                    : string.Empty;
                if (string.IsNullOrWhiteSpace(replacementValue) && 
                    node.Ancestors<InvocationExpressionSyntax>()
                    .Any(x => x.Expression.ToString().EndsWith("StartsWith") ||
                              x.Expression.ToString().EndsWith("EndsWith") ||
                              x.Expression.ToString().EndsWith("Contains")))
                {
                    replacementValue = "test-mutest-empty-string";
                }

                if (node.Parent != null && 
                    node.Parent.IsKind(SyntaxKind.CaseSwitchLabel) ||
                    node.Parent?.Parent != null && 
                    node.Parent.Parent.IsKind(SyntaxKind.CaseSwitchLabel))
                {
                    replacementValue = "mutest_case";
                }

                var replacementNode = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(replacementValue));

                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"String mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.String
                };
            }
        }
    }
}
