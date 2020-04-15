using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class MethodWithBoolArgument : IRule
    {
        private const string BoolKeyword = "bool";
        private const string BooleanKeyword = "Boolean";

        public string Description => "This method receives a bool argument. This is prone to be against SRP from SOLID";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Passing+bool+to+a+method+as+parameter";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is ParameterListSyntax root) ||
                root.Parent is ParenthesizedLambdaExpressionSyntax ||
                !root.Parameters.Any() ||
                root.Parameters.All(x =>
                {
                    var type = x.Type.ToString();
                    return type != BoolKeyword && type != BooleanKeyword;
                }))
            {
                return null;
            }

            var method = node.Ancestors<MethodDeclarationSyntax>().FirstOrDefault();
            if (method != null && method.AttributeLists.Any())
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
