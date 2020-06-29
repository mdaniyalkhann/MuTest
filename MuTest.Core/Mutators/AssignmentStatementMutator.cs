using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Mutants;
using MuTest.Core.Utility;

namespace MuTest.Core.Mutators
{
    public class AssignmentStatementMutator : Mutator<AssignmentExpressionSyntax>, IMutator
    {
        private static readonly IReadOnlyDictionary<SyntaxKind, SyntaxKind> KindsToMutate = new Dictionary<SyntaxKind, SyntaxKind>
        {
            [SyntaxKind.AddAssignmentExpression] = SyntaxKind.SubtractAssignmentExpression,
            [SyntaxKind.SubtractAssignmentExpression] = SyntaxKind.AddAssignmentExpression,
            [SyntaxKind.MultiplyAssignmentExpression] = SyntaxKind.DivideAssignmentExpression,
            [SyntaxKind.DivideAssignmentExpression] = SyntaxKind.MultiplyAssignmentExpression,
            [SyntaxKind.ModuloAssignmentExpression] = SyntaxKind.MultiplyAssignmentExpression,
            [SyntaxKind.AndAssignmentExpression] = SyntaxKind.ExclusiveOrAssignmentExpression,
            [SyntaxKind.ExclusiveOrAssignmentExpression] = SyntaxKind.AndAssignmentExpression,
            [SyntaxKind.LeftShiftAssignmentExpression] = SyntaxKind.RightShiftAssignmentExpression,
            [SyntaxKind.RightShiftAssignmentExpression] = SyntaxKind.LeftShiftAssignmentExpression
        };

        public override IEnumerable<Mutation> ApplyMutations(AssignmentExpressionSyntax node)
        {
            var assignmentKind = node.Kind();
            if (KindsToMutate.TryGetValue(assignmentKind, out var targetAssignmentKind))
            {
                var replacementNode = SyntaxFactory.AssignmentExpression(targetAssignmentKind, node.Left, node.Right);
                if (node.Kind() == SyntaxKind.AddAssignmentExpression
                    && (node.Left.IsAStringExpression() || node.Right.IsAStringExpression()))
                {
                    replacementNode = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, node.Left, node.Right);
                }

                yield return new Mutation
                {
                    OriginalNode = node,
                    ReplacementNode = replacementNode,
                    DisplayName = $"{assignmentKind} to {targetAssignmentKind} mutation - {node} replace with {replacementNode}",
                    Type = MutatorType.Assignment
                };
            }
        }

        public string Description { get; } = "ASSIGNMENT [+=, -=, x=, /=, %=, &=, |=, <<=, >>=]";

        public bool DefaultMutant { get; } = false;
    }
}