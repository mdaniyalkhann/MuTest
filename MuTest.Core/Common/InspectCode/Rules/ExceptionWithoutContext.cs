using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class ExceptionWithoutContext : IRule
    {
        public string Description => "This exception message does not provide context description.";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Avoid+throwing+exceptions+with+no+context+message";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            var root = node as ObjectCreationExpressionSyntax;

            var exceptionWithoutContext = root?.Parent is ThrowStatementSyntax &&
                                          !root.ArgumentList.Arguments.Any();

            if (root == null || !exceptionWithoutContext)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
