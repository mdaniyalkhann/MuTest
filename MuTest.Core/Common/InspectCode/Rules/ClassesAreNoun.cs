using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class ClassesAreNoun : IRule
    {
        public string Description => @"Classes are nouns. Rename this class in order to eliminate ""Processor"", ""Data"" or ""Info"" word";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/mxCRF";

        public string Severity => Constants.SeverityType.Yellow.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            var root = node as ClassDeclarationSyntax;
            var keywords = new[]
            {
                "Manager",
                "Processor",
                "Data",
                "Info"
            };

            if (root != null &&
                keywords.Any(x => root.Identifier.ValueText.EndsWith(x)))
            {
                return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
            }

            return null;
        }
    }
}
