using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class TestEntireUow : IRule
    {
        public string Description => "Shouldly - Test entire UnitOfWork";

        public string CodeReviewUrl => "https://confluence.devfactory.com/pages/viewpage.action?pageId=352262571";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax syntax) ||
                syntax.DescendantNodes<IdentifierNameSyntax>()
                    .Any(x => x.Identifier.ValueText.Equals("ShouldSatisfyAllConditions", StringComparison.InvariantCulture)))
            {
                return null;
            }

            return syntax.DescendantNodes<IdentifierNameSyntax>()
                       .Count(x => x.Identifier.ValueText.StartsWith("ShouldBe") ||
                                   x.Identifier.ValueText.StartsWith("ShouldNotBe") ||
                                   x.Identifier.ValueText.StartsWith("ShouldThrow")) <= 2
                ? null
                : Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}