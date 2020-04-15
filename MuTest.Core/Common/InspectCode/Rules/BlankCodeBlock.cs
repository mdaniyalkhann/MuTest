using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class BlankCodeBlock : IRule
    {
        public string Description { get; set; }

        public string CodeReviewUrl { get; set; }

        public string Severity { get; set; }  = Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is BlockSyntax root) ||
                root.DescendantNodes().Any() ||
                root.Parent is SimpleLambdaExpressionSyntax ||
                root.Parent is LambdaExpressionSyntax ||
                root.Parent is ParenthesizedLambdaExpressionSyntax)
            {
                return null;
            }

            Description = "This code has a blank block to do nothing. Sometimes this means the code missed to implement here";
            CodeReviewUrl = "https://confluence.devfactory.com/display/CodeReview/Blank+code+block";

            if (root.Parent is IfStatementSyntax)
            {
                Description = "This method contains an unnecessary empty if statement";
                Severity = Constants.SeverityType.Yellow.ToString();
                CodeReviewUrl = "https://confluence.devfactory.com/display/CodeReview/Empty+and+unnecessary+if+statement";
            }
            else if (root.Parent is CatchClauseSyntax)
            {
                Description = @"The exception is ignored (""swallowed"") by the try-catch block.";
                CodeReviewUrl = "https://confluence.devfactory.com/display/CodeReview/Catching+and+Ignoring+Exception";
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
        }
    }
}
