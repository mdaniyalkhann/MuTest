using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class GeneralReservedException : IRule
    {
        public string Description => "A method raises an exception type that is too general or that is reserved by the runtime. Use specific or Aggregation Exception";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Reserved+exceptions";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            var root = node as IdentifierNameSyntax;
            var genericExceptions = new[]
            {
                "Exception",
                "ApplicationException",
                "SystemException",
                "ExecutionEngineException",
                "IndexOutOfRangeException",
                "NullReferenceException",
                "OutOfMemoryException"
            };

            if (root == null ||
                genericExceptions.All(x => root.Identifier.ValueText != x) ||
                !(root.Parent is ObjectCreationExpressionSyntax &&
                  root.Parent.Parent is ThrowStatementSyntax))
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
