using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class DuplicateShimsDefinition : IRule
    {
        public string Description => "Remove Duplicate Shims Blocks";

        public string CodeReviewUrl => "n/a";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is MemberAccessExpressionSyntax))
            {
                return null;
            }

            if (!node.ToString().Contains(".AllInstances."))
            {
                return null;
            }

            var method = node.Ancestors<MethodDeclarationSyntax>().FirstOrDefault();
            return method?
                       .DescendantNodes<MemberAccessExpressionSyntax>()
                       .Count(x => x.ToString() == node.ToString()) > 1
                ? Inspection.Create(this, node.LineNumber() + 1, node.ToFullString())
                : null;
        }
    }
}
