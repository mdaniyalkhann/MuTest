using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class InappropriateUsageOfProperty : IRule
    {
        public string Description => "Inappropriate usage of property";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Inappropriate+usage+of+property";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is PropertyDeclarationSyntax property))
            {
                return null;
            }

            if (property.DescendantNodes<ArrowExpressionClauseSyntax>().Count < 4)
            {
                return null;
            }

            if (!(property.DescendantNodes<InvocationExpressionSyntax>().Any() ||
                  property.DescendantNodes<ObjectCreationExpressionSyntax>().Any()))
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}