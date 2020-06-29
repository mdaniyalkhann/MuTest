using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class UnnecessaryTestCase : IRule
    {
        public string Description => "Simplify Test Code by removing single Test Case or by removing same arguments across all test cases";

        public string CodeReviewUrl => "n/a";

        public string Severity => SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is MethodDeclarationSyntax method))
            {
                return null;
            }

            var attributes = method.AttributeLists.Where(x => x.Attributes.Any(y => y.Name.ToString() == "TestCase")).SelectMany(x => x.Attributes).ToList();

            if (!attributes.Any())
            {
                return null;
            }

            if (attributes.Count == 1)
            {
                return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
            }

            var firstAttribute = attributes.First();
            for (var attrArgIndex = 0; attrArgIndex < firstAttribute.ArgumentList.Arguments.Count; attrArgIndex++)
            {
                if (attributes.Skip(1).All(x => x.ArgumentList.Arguments[attrArgIndex].ToString() ==
                                                firstAttribute.ArgumentList.Arguments[attrArgIndex].ToString()))
                {
                    return Inspection.Create(this, node.LineNumber() + 1, node.ToFullString());
                }
            }

            return null;
        }
    }
}
