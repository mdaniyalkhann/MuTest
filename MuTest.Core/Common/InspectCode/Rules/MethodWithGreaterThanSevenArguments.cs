using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class MethodWithGreaterThanSevenArguments : IRule
    {
        public string Description => "This method receives too many parameters (> 7)";

        public string CodeReviewUrl => "https://confluence.devfactory.com/x/Tw15F";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            if (!(node is ParameterListSyntax root) ||
                root.Parent is ParenthesizedLambdaExpressionSyntax ||
                root.Parameters.Count <= 7)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
