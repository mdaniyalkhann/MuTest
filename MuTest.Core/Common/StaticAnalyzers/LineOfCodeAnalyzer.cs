using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MuTest.Core.Common.StaticAnalyzers
{
    public static class LineOfCodeAnalyzer
    {
        public static int CalculateLoc(this SyntaxNode x)
        {
            if (x == null)
            {
                return 1;
            }

            var nodes = x.DescendantNodes().ToList();
            var loc = nodes.Count(y => y.Parent is BlockSyntax ||
                                       y.Parent is IfStatementSyntax ||
                                       y.Parent is ElseClauseSyntax ||
                                       y.Parent is SwitchSectionSyntax ||
                                       y.Parent is WhileStatementSyntax ||
                                       y.Parent is ForEachStatementSyntax ||
                                       y.Parent is ForEachVariableStatementSyntax ||
                                       y.Parent is ForStatementSyntax ||
                                       y.Parent is DoStatementSyntax ||
                                       y.Parent is TryStatementSyntax ||
                                       y.Parent is CatchClauseSyntax);

            loc = loc +
                  nodes.Count(y => y is SwitchStatementSyntax) -
                  nodes.Count(y => y is ForStatementSyntax ||
                                   y is WhileStatementSyntax ||
                                   y is BlockSyntax);
            return loc;
        }
    }
}
