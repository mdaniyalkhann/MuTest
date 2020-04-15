using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class SwitchWithoutDefaultCase : IRule
    {
        public string Description => "Missing default case for switch";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Missing+default+case+for+switch";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is SwitchStatementSyntax root) ||
                root.DescendantNodes()
                    .OfType<DefaultSwitchLabelSyntax>().Any())
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
