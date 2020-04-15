using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class ProtectedPublicConstants : IRule
    {
        private const string PublicModifier = "public";
        private const string ProtectedModifier = "protected";
        private const string ConstantModifier = "const";

        public string Description => "Avoid protected / public constants for values that might change";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/uIw-Ew";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is FieldDeclarationSyntax field))
            {
                return null;
            }

            var privateField = !(field.Modifiers.ToString().Contains(PublicModifier) || 
                                field.Modifiers.ToString().Contains(ProtectedModifier));
            if (privateField)
            {
                return null;
            }

            var containConstant = field.Modifiers.ToString().Contains(ConstantModifier);
            if (!containConstant)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
