using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;

namespace MuTest.Core.Mutators
{
    public class NonVoidMethodCallMutator : Mutator<InvocationExpressionSyntax>, IMutator
    {
        public string Description { get; } = "NON-VOID METHOD CALL";

        public bool DefaultMutant { get; } = false;

        public override IEnumerable<Mutation> ApplyMutations(InvocationExpressionSyntax node)
        {
            SyntaxNode originalNode = null;
            if (node.Parent.Parent is ExpressionStatementSyntax &&
                node.Parent is AssignmentExpressionSyntax)
            {
                originalNode = node.Parent.Parent;
            }

            if (node.Parent.Parent.Parent is ExpressionStatementSyntax &&
                node.Parent.Parent is AssignmentExpressionSyntax)
            {
                originalNode = node.Parent.Parent.Parent;
            }

            if (originalNode != null)
            {
                var replacementNode = SyntaxFactory.EmptyStatement();
                yield return new Mutation
                {
                    OriginalNode = originalNode,
                    ReplacementNode = replacementNode,
                    DisplayName = $"Non-Void Method Call mutation - remove {originalNode}",
                    Type = MutatorType.MethodCall
                };
            }
        }
    }
}