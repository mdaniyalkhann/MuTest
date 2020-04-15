using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.Model;
using MuTest.Core.Utility;

namespace MuTest.Core.Common.StaticAnalyzers
{
    public class MethodAnalyzer
    {
        private double _totalNumberOfMutants;
        private double _totalNumberOfMissedMutants;

        public StringBuilder FindMutants(
            SyntaxNode testClassDeclaration,
            IList<MethodDetail> selectedMethods)
        {
            if (testClassDeclaration == null)
            {
                throw new ArgumentNullException(nameof(testClassDeclaration));
            }

            if (selectedMethods == null)
            {
                throw new ArgumentNullException(nameof(selectedMethods));
            }

            var methodBuilder = new StringBuilder();
            var methodDetails = selectedMethods.ToList();
            _totalNumberOfMutants = 0;
            _totalNumberOfMissedMutants = 0;

            foreach (var method in methodDetails)
            {
                var mSyntax = method.Method;
                var asserts = mSyntax.GetAsserts();

                methodBuilder.AppendLine("<fieldset style=\"margin-bottom:30\">");
                methodBuilder.AppendLine("<legend>");
                methodBuilder.AppendLine($@"Method Name: {method.MethodName}".PrintWithPreTagImportant());
                methodBuilder.AppendLine("</legend>");
                methodBuilder.AppendLine($"Complexity: {mSyntax.GetComplexity()}".PrintWithPreTagImportant(color: Constants.Colors.Green));
                methodBuilder.AppendLine($"Line of Codes: {mSyntax.CalculateLoc()}".PrintWithPreTagImportant(color: Constants.Colors.Green));
                methodBuilder.AppendLine("Required Asserts: ".PrintWithPreTagImportant(color: Constants.Colors.Green));
                methodBuilder.AppendLine(asserts.Print().ToString().Encode().PrintWithPreTagWithMargin());
                methodBuilder.AppendLine("Mutants: ".PrintWithPreTagImportant(color: Constants.Colors.Green));
                methodBuilder.AppendLine(mSyntax.AnalyzeMutants().ToString());
                methodBuilder.AppendLine("Mutants Missed By Tests: ".PrintWithPreTagImportant(color: Constants.Colors.Green));
                methodBuilder.AppendLine(FindMissedMutants(testClassDeclaration, method, asserts).ToString());
                methodBuilder.AppendLine("</fieldset>");
            }

            var coverage = 1 - _totalNumberOfMissedMutants / _totalNumberOfMutants;
            methodBuilder.AppendLine(
                $"Mutants Coverage (Total): [{_totalNumberOfMissedMutants} Missed/{_totalNumberOfMutants}]({coverage:P})".PrintWithPreTagImportant(color: Constants.Colors.Brown));

            return methodBuilder;
        }

        private StringBuilder FindMissedMutants(SyntaxNode testClassDeclaration,
            MethodDetail methodDetail,
            IList<AssertsAnalyzer.AssertMapper> asserts)
        {
            var testBuilder = new StringBuilder(Constants.PreStartWithMargin);
            double numberOfMutants = 0;
            double numberOfMissedMutants = 0;
            var testClass = testClassDeclaration.ToFullString();

            foreach (var methodNode in methodDetail.Method.DescendantNodes())
            {
                if (methodNode is InvocationExpressionSyntax invocation)
                {
                    foreach (var argument in invocation.ArgumentList.Arguments)
                    {
                        numberOfMutants++;
                        var argExpression = argument.Expression.ToString().Encode();
                        var methodExpression = invocation.GetMethod().ToString().Encode().Print(color: Constants.Colors.Red);
                        if ((argExpression.StartsWith(@"""") ||
                             argExpression.StartsWith(@"'") ||
                             argExpression.IsNumeric()) &&
                            !testClass.Contains(argExpression))
                        {
                            testBuilder
                                .AppendLine($"{(argExpression + " - ").Print(color: Constants.Colors.Blue)}{methodExpression}");
                            numberOfMissedMutants++;
                        }
                        else if (argExpression.Contains(".") &&
                                 !testClass.Contains(argExpression.Split('.').Last()))
                        {
                            testBuilder
                                .AppendLine(
                                    $"{(argExpression + " - ").Print(color: Constants.Colors.Blue)}{methodExpression}");
                            numberOfMissedMutants++;
                        }
                    }
                }
            }

            foreach (var assertMapper in asserts)
            {
                numberOfMutants++;
                var expected = assertMapper.Right.RemoveUnnecessaryWords();
                var actual = assertMapper.Left.RemoveUnnecessaryWords();

                var isNullOrWhiteSpace = string.IsNullOrWhiteSpace(expected.Replace("\"", string.Empty)) || expected == "null";
                if (isNullOrWhiteSpace)
                {
                    if (!testClass.Contains(actual))
                    {
                        testBuilder.AppendLine(assertMapper.Print().Encode().Print(color: Constants.Colors.Red));
                        numberOfMissedMutants++;
                    }
                }
                else if (!testClass.Contains(expected))
                {
                    testBuilder.AppendLine(assertMapper.Print().Encode().Print(color: Constants.Colors.Red));
                    numberOfMissedMutants++;
                }
            }

            var coverage = 1 - numberOfMissedMutants / numberOfMutants;
            testBuilder.AppendLine($"Mutants Coverage: [{numberOfMissedMutants} Missed/{numberOfMutants}]({coverage:P})".PrintWithPreTagImportant(color: Constants.Colors.Orange));
            testBuilder.AppendLine(Constants.PreEnd);

            _totalNumberOfMutants += numberOfMutants;
            _totalNumberOfMissedMutants += numberOfMissedMutants;
            return testBuilder;
        }
    }
}