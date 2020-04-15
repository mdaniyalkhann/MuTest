using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class FieldAsProtectedOrPublic : IRule
    {
        public string Description => "This code exposes a field as public or protected. Encapsulate this field into a property";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/kSCkEw";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is FieldDeclarationSyntax root) ||
                root.Modifiers.ToString().Contains("private") ||
                root.Modifiers.ToString().Contains("internal") ||
                root.Modifiers.ToString().Contains("const") ||
                root.Modifiers.ToString().Contains("static") ||
                string.IsNullOrWhiteSpace(root.Modifiers.ToString()) ||
                root.Declaration.Variables.Count > 1)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
