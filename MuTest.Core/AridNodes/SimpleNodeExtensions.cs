using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes
{
    public static class SimpleNodeExtensions
    {
        public static bool IsInvocationOfMemberOfType(this SimpleNode simpleNode, Type type)
        {
            if (simpleNode == null)
            {
                throw new ArgumentOutOfRangeException(nameof(simpleNode));
            }
            return simpleNode.SyntaxNode is InvocationExpressionSyntax invocationExpression
                   && invocationExpression.Expression is MemberAccessExpressionSyntax memberAccessExpression
                   && memberAccessExpression.Expression is IdentifierNameSyntax identifierNameSyntax
                   && identifierNameSyntax.Identifier.Text == type.Name;
        }
    }
}