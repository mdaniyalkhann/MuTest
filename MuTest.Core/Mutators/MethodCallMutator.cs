using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class MethodCallMutator : Mutator<InvocationExpressionSyntax>, IMutator
    {
        public string Description { get; } = "VOID METHOD CALL";

        public bool DefaultMutant { get; } = false;

        public override IEnumerable<Mutation> ApplyMutations(InvocationExpressionSyntax node)
        {
            if (node.Parent is ExpressionStatementSyntax || 
                node.Parent.Parent is ExpressionStatementSyntax &&
                node.Parent is ConditionalAccessExpressionSyntax)
            {
                var originalNode = node.Parent;
                if (node.Parent is ConditionalAccessExpressionSyntax)
                {
                    originalNode = node.Parent.Parent;
                }

                var replacementNode = SyntaxFactory.EmptyStatement();
                yield return new Mutation
                {
                    OriginalNode = originalNode,
                    ReplacementNode = replacementNode,
                    DisplayName = $"Void Method Call mutation - remove {originalNode}",
                    Type = MutatorType.MethodCall
                };
            }
        }
    }
}