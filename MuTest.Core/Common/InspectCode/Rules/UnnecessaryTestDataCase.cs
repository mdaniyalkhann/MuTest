using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class UnnecessaryTestDataCase : IRule
    {
        public string Description => "Simplify Test Code by removing single Test Case or by removing same arguments across all test cases";

        public string CodeReviewUrl => "n/a";

        public string Severity => SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is InitializerExpressionSyntax initializer))
            {
                return null;
            }

            if (!initializer.ChildNodes().All(y => y.Kind() == SyntaxKind.ObjectCreationExpression &&
                                                   (y as ObjectCreationExpressionSyntax)?.Type?.ToString() == "TestCaseData"))
            {
                return null;
            }

            var testCases = initializer.ChildNodes().Cast<ObjectCreationExpressionSyntax>().ToList();
            if (!testCases.Any())
            {
                return null;
            }

            if (testCases.Count == 1)
            {
                return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
            }

            var firstAttribute = testCases.First();
            for (var attrArgIndex = 0; attrArgIndex < firstAttribute.ArgumentList.Arguments.Count; attrArgIndex++)
            {
                if (testCases.Skip(1).All(x => x.ArgumentList.Arguments[attrArgIndex].ToString() ==
                                               firstAttribute.ArgumentList.Arguments[attrArgIndex].ToString()))
                {
                    return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
                }
            }

            return null;
        }
    }
}
