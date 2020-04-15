using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class AssertSingleItemWithUow : IRule
    {
        public string Description => "Shouldly - Avoid Assert Single Item With UnitOfWork";

        public string CodeReviewUrl => "https://confluence.devfactory.com/pages/viewpage.action?pageId=352262571";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is IdentifierNameSyntax syntax) || !syntax.Identifier.ValueText.Equals("ShouldSatisfyAllConditions", StringComparison.InvariantCulture))
            {
                return null;
            }

            var invocation = node.Ancestors<InvocationExpressionSyntax>().FirstOrDefault();
            if (invocation == null || invocation.ArgumentList.Arguments.Count > 1)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
