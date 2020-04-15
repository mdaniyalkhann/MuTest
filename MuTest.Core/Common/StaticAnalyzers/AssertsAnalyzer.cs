using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.StaticAnalyzers
{
    public static class AssertsAnalyzer
    {
        private const string AddItem = ".Add";

        public static IList<AssertMapper> GetAsserts(this SyntaxNode mSyntax)
        {
            if (mSyntax == null)
            {
                throw new ArgumentNullException(nameof(mSyntax));
            }

            var variables = mSyntax.Ancestors<ClassDeclarationSyntax>().FirstOrDefault()?
                .DescendantNodes<VariableDeclaratorSyntax>()
                .Where(x => x.Initializer != null &&
                            x.Ancestors<FieldDeclarationSyntax>().Any())
                .ToList();

            variables?.AddRange(mSyntax.DescendantNodes<VariableDeclaratorSyntax>()
                .Where(x => x.Initializer != null));

            var nodes = mSyntax.DescendantNodes<AssignmentExpressionSyntax>();
            var forEachNodes = mSyntax.DescendantNodes<ForEachStatementSyntax>().ToList();
            var addMethodNodes = mSyntax.DescendantNodes<InvocationExpressionSyntax>()
                .Where(x => x.Expression.ToString().EndsWith(AddItem) && x.ArgumentList.Arguments.Count == 1)
                .ToList();

            var forEachNameValues = new NameValueCollection();
            var addMethodNameValues = new NameValueCollection();
            var variablesNameValues = new NameValueCollection();

            variables?.ForEach(x => variablesNameValues
                .Add(x.Identifier.ValueText, x.Initializer.Value.ToString()));

            forEachNodes.ForEach(x =>
            {
                var variableName = x.Identifier.ValueText;
                if (forEachNameValues[variableName] == null)
                {
                    forEachNameValues.Add(variableName, $"{x.Expression}.First()");
                }
            });

            addMethodNodes.ForEach(x =>
            {
                var variableName = x.ArgumentList.Arguments[0].ToString();
                if (addMethodNameValues[variableName] == null)
                {
                    var expression = x.Expression.ToString().Replace(AddItem, string.Empty);
                    var index = 0;
                    foreach (string key in addMethodNameValues.Keys)
                    {
                        var keyValue = addMethodNameValues[key];
                        if (keyValue.Substring(0, keyValue.IndexOf("[", StringComparison.Ordinal)) == expression)
                        {
                            index = int.Parse(
                                        keyValue.Substring(
                                            keyValue.IndexOf("[", StringComparison.Ordinal) + 1,
                                            keyValue.Length - keyValue.IndexOf("]", StringComparison.Ordinal))) + 1;
                            break;
                        }
                    }

                    addMethodNameValues.Add(variableName, $"{expression}[{index}]");
                }
            });

            var assetExpression = new List<AssertMapper>();
            foreach (var node in nodes)
            {
                var left = node.Left.ToString();
                var right = node.Right.ToString();

                if (node.Right is LiteralExpressionSyntax ||
                    node.Right is MemberAccessExpressionSyntax ||
                    node.Right is IdentifierNameSyntax)
                {
                    if (node.Right is IdentifierNameSyntax)
                    {
                        var complexVariable = variables?
                            .FirstOrDefault(x => x.Identifier.ValueText == node.Right.ToString())?
                            .Initializer
                            .DescendantNodes<InvocationExpressionSyntax>().Any();

                        if (complexVariable.GetValueOrDefault())
                        {
                            continue;
                        }
                    }

                    if (node.Parent is InitializerExpressionSyntax)
                    {
                        if (node.Ancestors<InvocationExpressionSyntax>().Any())
                        {
                            var invocationExpression = node.Ancestors<InvocationExpressionSyntax>().First().Expression.ToString();
                            if (invocationExpression.EndsWith(AddItem))
                            {
                                left = $"{invocationExpression.Replace(AddItem, string.Empty)}.First().{left}";
                            }
                        }
                    }

                    var forEachLeftValue = forEachNameValues.AllKeys.FirstOrDefault(x => left.StartsWith(x + "."));
                    var addMethodLeftValue = addMethodNameValues.AllKeys.FirstOrDefault(x => left.StartsWith(x + "."));
                    var forEachRightValue = forEachNameValues.AllKeys.FirstOrDefault(x => right.StartsWith(x + "."));
                    var addMethodRightValue = addMethodNameValues.AllKeys.FirstOrDefault(x => right.StartsWith(x + "."));

                    assetExpression.Add(new AssertMapper
                    {
                        Left = forEachLeftValue != null
                            ? left.Replace($"{forEachLeftValue}.", $"{forEachNameValues[forEachLeftValue]}.")
                            : addMethodLeftValue != null
                                ? left.Replace($"{addMethodLeftValue}.", $"{addMethodNameValues[addMethodLeftValue]}.")
                                : left,
                        Right = variablesNameValues.Get(right.RemoveUnnecessaryWords()) != null
                            ? variablesNameValues[right.RemoveUnnecessaryWords()]
                            : forEachRightValue != null
                                ? right.Replace($"{forEachRightValue}.", $"{forEachNameValues[forEachRightValue]}.")
                                : addMethodLeftValue != null
                                    ? right.Replace($"{addMethodRightValue}.", $"{addMethodNameValues[addMethodRightValue]}.")
                                    : right
                    });
                }
            }

            assetExpression = assetExpression.OrderBy(x => x.Left + x.Right).ToList();
            assetExpression = assetExpression.GroupBy(x => new { x.Left, x.Right })
                .Select(x => new AssertMapper
                {
                    Left = x.Key.Left,
                    Right = x.Key.Right
                }).ToList();


            return assetExpression;
        }

        public static StringBuilder Print(this IList<AssertMapper> asserts)
        {
            if (asserts == null)
            {
                throw new ArgumentNullException(nameof(asserts));
            }

            var assertBuilder = new StringBuilder();

            asserts.ToList().ForEach(x => assertBuilder.AppendLine(Print(x)));

            return assertBuilder;
        }

        public static string Print(this AssertMapper assert)
        {
            if (assert == null)
            {
                throw new ArgumentNullException(nameof(assert));
            }

            return GetAssertion(assert.Left.RemoveUnnecessaryWords(), assert.Right.RemoveUnnecessaryWords());
        }

        private static string GetAssertion(string left, string right)
        {
            switch (right)
            {
                case "true":
                    return $"{left}.ShouldBeTrue()";
                case "false":
                    return $"{left}.ShouldBeFalse()";
                case "null":
                    return $"{left}.ShouldBeNull()";
                case "":
                case "\"\"":
                case "string.Empty":
                case " ":
                    return $"{left}.ShouldBeNullOrWhiteSpace()";
                default:
                    return $"{left}.ShouldBe({right})";
            }
        }

        public class AssertMapper
        {
            public string Left { get; set; }
            public string Right { get; set; }
        }
    }
}