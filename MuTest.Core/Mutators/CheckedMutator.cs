using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class CheckedMutator : Mutator<CheckedExpressionSyntax>, IMutator
    {
        public string Description { get; } = "CHECKED (checked, unchecked)";

        public bool DefaultMutant { get; } = false;

        public override IEnumerable<Mutation> ApplyMutations(CheckedExpressionSyntax node)
        {
            if (node.Kind() == SyntaxKind.CheckedExpression)
            {
                yield return new Mutation()
                {
                    OriginalNode = node,
                    ReplacementNode = node.Expression,
                    DisplayName = $"Remove checked expression - {node} replace with {node.Expression}",
                    Type = MutatorType.Checked
                };
            }
        }
    }
}
