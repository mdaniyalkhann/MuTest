using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Common.StaticAnalyzers
{
    public static class ComplexityAnalyzer
    {
        private static readonly IReadOnlyDictionary<Type, string> Blocks = new Dictionary<Type, string>
        {
            [typeof(IfStatementSyntax)] = "If",
            [typeof(ElseClauseSyntax)] = "Else",
            [typeof(SwitchStatementSyntax)] = "Switch",
            [typeof(WhileStatementSyntax)] = "While",
            [typeof(ForStatementSyntax)] = "For",
            [typeof(ForEachStatementSyntax)] = "ForEach",
            [typeof(DoStatementSyntax)] = "Do",
            [typeof(TryStatementSyntax)] = "Try",
            [typeof(BreakStatementSyntax)] = "Break",
            [typeof(ContinueStatementSyntax)] = "Continue",
            [typeof(SelectClauseSyntax)] = "Select",
            [typeof(WhereClauseSyntax)] = "Where",
            [typeof(FromClauseSyntax)] = "From",
            [typeof(JoinClauseSyntax)] = "Join",
            [typeof(GroupClauseSyntax)] = "Group",
            [typeof(ReturnStatementSyntax)] = "Return",
            [typeof(OrderByClauseSyntax)] = "OrderBy",
            [typeof(FinallyClauseSyntax)] = "Finally",
            [typeof(QueryClauseSyntax)] = "Query",
            [typeof(CatchClauseSyntax)] = "Catch",
            [typeof(LockStatementSyntax)] = "Lock",
            [typeof(YieldStatementSyntax)] = "Yield",
            [typeof(ThrowStatementSyntax)] = "Throw"
        };

        public static string GetComplexity(this SyntaxNode method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var pathBuilder = new StringBuilder();
            foreach (var node in method.DescendantNodes())
            {
                if (Blocks.ContainsKey(node.GetType()))
                {
                    pathBuilder.Append($"{Blocks[node.GetType()]}->");
                }
            }

            var pathResult = pathBuilder.ToString().Trim('>').Trim('-');
            pathResult = string.IsNullOrWhiteSpace(pathResult) ? "Empty" : pathResult;

            return pathResult;
        }
    }
}
