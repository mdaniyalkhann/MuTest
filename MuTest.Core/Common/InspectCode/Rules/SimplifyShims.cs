using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class SimplifyShims : IRule
    {
        public string Description => "Simplify Shims using existing object instances";

        public string CodeReviewUrl => "n/a";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is MemberAccessExpressionSyntax))
            {
                return null;
            }

            if (!node.ToString().EndsWith("AllInstances"))
            {
                return null;
            }

            var method = node.Ancestors<MethodDeclarationSyntax>().FirstOrDefault();
            return method?.DescendantNodes<ObjectCreationExpressionSyntax>()
                       .Any(obj => node.ToString() == $"{obj.Type}.AllInstances") == true
                ? Inspection.Create(this, node.LineNumber() + 1, node.ToFullString())
                : null;
        }
    }
}