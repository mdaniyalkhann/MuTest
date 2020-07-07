using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class UnnecessaryUseOfShimsContext : IRule
    {
        public string Description => "Remove Unnecessary Shims Contexts";

        public string CodeReviewUrl => "n/a";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode context)
        {
            if (!(context is UsingStatementSyntax node))
            {
                return null;
            }

            if (!node.Expression.ToString().Contains("ShimsContext.Create()"))
            {
                return null;
            }

            return node.Statement.ToString().Contains("Shim") 
                ? null 
                : Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}