using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class EnumWithoutDefaultValue : IRule
    {
        public string Description => "This enumeration does not contain a value for 0 (zero).";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Missing+default+value+for+enum";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is EnumDeclarationSyntax root) ||
                root.Members.Any(x => x.EqualsValue.Value.ToString() == "0"))
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
