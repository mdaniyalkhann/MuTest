using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class ArrayToListInForEach : IRule
    {
        public string Description => "ToArray/ToList inside foreach declaration. Remove the `ToArray()` or `ToList()` call and use the `IEnumerable` instance directly";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/bQyUEw";

        public string Severity => Constants.SeverityType.Severe.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is ForEachStatementSyntax root))
            {
                return null;
            }

            var enumerable = root.Expression.ToString();

            if (!(enumerable.Contains(".ToArray()") ||
                  enumerable.Contains(".ToList()")))
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
