using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.StaticAnalyzers
{
    public static class MutantsAnalyzer
    {
        public static StringBuilder AnalyzeMutants(this SyntaxNode mSyntax)
        {
            if (mSyntax == null)
            {
                throw new ArgumentNullException(nameof(mSyntax));
            }

            var conditionalMutantsCount = 0;
            var subMethodMutantsCount = 0;
            var methodBuilder = new StringBuilder();
            foreach (var methodNode in mSyntax.DescendantNodes())
            {
                if (methodNode is BinaryExpressionSyntax &&
                    methodNode.Parent is IfStatementSyntax)
                {
                    conditionalMutantsCount++;
                    methodBuilder.AppendLine($"If {methodNode.ToString().Encode()}".PrintWithPreTagWithMargin(Constants.FontSizeTwo, Constants.Colors.BlueViolet));
                }

                if (methodNode is BinaryExpressionSyntax &&
                    methodNode.Parent is ElseClauseSyntax)
                {
                    conditionalMutantsCount++;
                    methodBuilder.AppendLine($"Else If {methodNode.ToString().Encode()}".PrintWithPreTagWithMargin(Constants.FontSizeTwo, Constants.Colors.BlueViolet));
                }

                if (methodNode is InvocationExpressionSyntax invocation)
                {
                    subMethodMutantsCount++;
                    methodBuilder.AppendLine(GetMethod(invocation).ToString().Encode().PrintWithPreTagWithMargin(Constants.FontSizeTwo, Constants.Colors.Blue));
                }
            }

            methodBuilder
                .AppendLine($"Number of Conditional Mutants: {conditionalMutantsCount}".PrintWithPreTag(color: Constants.Colors.BlueViolet))
                .AppendLine($"Number of Sub Method Mutants: {subMethodMutantsCount}".PrintWithPreTag(color: Constants.Colors.Blue));

            return methodBuilder;
        }

        public static StringBuilder GetMethod(this InvocationExpressionSyntax invocation)
        {
            if (invocation == null)
            {
                throw new ArgumentNullException(nameof(invocation));
            }

            var invocationBuilder = new StringBuilder(invocation.Expression.ToString()).Append("(");
            var argumentsCount = invocation.ArgumentList.Arguments.Count;
            for (var index = 0; index < argumentsCount; index++)
            {
                var arg = invocation.ArgumentList.Arguments[index];
                invocationBuilder.Append(arg.Expression);

                if (index < argumentsCount - 1)
                {
                    invocationBuilder.Append(", ");
                }
            }

            invocationBuilder.Append(")");
            return invocationBuilder;
        }
    }
}
