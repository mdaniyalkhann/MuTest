using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.InspectCode.Rules
{
    public class ContextualKeyword : IRule
    {
        public string Description => "This code uses the contextual keyword as a variable or member name.";

        public string CodeReviewUrl => "https://confluence.devfactory.com/display/CodeReview/Contextual+keyword";

        public string Severity => Constants.SeverityType.Red.ToString();

        public Inspection Analyze(SyntaxNode node)
        {
            var root = node as IdentifierNameSyntax;
            const string valueKeyWord = "value";
            var contextualKeywords = new[]
            {
                "add",
                "alias",
                "ascending",
                "async",
                "await",
                "by",
                "descending",
                "dynamic",
                "equals",
                "from",
                "get",
                "global",
                "group",
                "into",
                "join",
                "let",
                "nameof",
                "on",
                "orderby",
                "partial",
                "remove",
                "select",
                "set",
                valueKeyWord,
                "var",
                "when",
                "where",
                "yield"
            };

            if (root == null ||
                root.Parent is InvocationExpressionSyntax ||
                root.Parent is VariableDeclarationSyntax ||
                root.Parent is ForEachStatementSyntax ||
                root.Parent is ForStatementSyntax ||
                root.Parent is WhileStatementSyntax ||
                root.Parent is SwitchStatementSyntax ||
                root.Parent is DeclarationExpressionSyntax ||
                root.Parent is TypeOfExpressionSyntax ||
                root.Parent is MethodDeclarationSyntax ||
                root.Parent is ParameterSyntax ||
                root.Parent.Parent is GenericNameSyntax ||
                contextualKeywords.All(x => x != root.Identifier.ValueText))
            {
                return null;
            }

            if (root.Ancestors().Any(x => x.IsKind(SyntaxKind.SetAccessorDeclaration)) &&
                root.Identifier.ValueText == valueKeyWord)
            {
                return null;
            }

            return Inspection.Create(this, node.LineNumber() + 1, node.Parent.ToFullString());
        }
    }
}
