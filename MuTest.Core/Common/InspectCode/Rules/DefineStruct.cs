using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class DefineStruct : IRule
    {
        public string Description => "This code defines a `struct` that should be a `class`";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Prefer+class+instead+of+struct";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is StructDeclarationSyntax))
            {
                return null;
            }
            
            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
