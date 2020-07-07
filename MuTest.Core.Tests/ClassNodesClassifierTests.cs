using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.AridNodes;
using MuTest.Core.Model.AridNodes;
using MuTest.Core.Tests.Samples;
using MuTest.Core.Tests.Utility;
using NUnit.Framework;
using Shouldly;

namespace MuTest.Core.Tests
{
    /// <summary>
    /// <see cref="ClassNodesClassifier"/>
    /// </summary>
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ClassNodesClassifierTests
    {
        private const string SampleClassRelativePath = @"Samples\AridNodesSampleClass.cs";
        private static readonly ClassNodesClassifier Classifier = new ClassNodesClassifier();

        [Test]
        public void Check_WhenNodeIsSimpleBinaryExpression_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingSingleBinaryExpression)
                .GetFirstSyntaxNodeOfMethodFunc<BinaryExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeFalse(),
                () => result.TriggeredBy.ShouldBeEmpty());
        }

        [Test]
        public void Check_WhenNodeIsDebugNode_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingSingleDiagnosticsNode)
                .GetFirstSyntaxNodeOfMethodFunc<InvocationExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenLiteralIsArgumentOfDebugNode_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingSingleDiagnosticsNode)
                .GetFirstSyntaxNodeOfMethodFunc<LiteralExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenNodeIsConsoleNode_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingSingleConsoleNode)
                .GetFirstSyntaxNodeOfMethodFunc<InvocationExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenLoopStatementIsOnlyContainingDebugStatements_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingLoopWithOnlyDiagnosticsNode)
                .GetFirstSyntaxNodeOfMethodFunc<ForStatementSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_IfStatementIsOnlyContainingDebugStatements_ShouldNotBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingIfStatementWithOnlyDiagnosticsNode)
                .GetFirstSyntaxNodeOfMethodFunc<IfStatementSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeFalse(),
                () => result.TriggeredBy.ShouldBeEmpty());
        }

        [Test]
        public void Check_LiteralArgumentOfMethod_ShouldBeArid()
        {
            // Arrange
            var getSyntaxNode = nameof(AridNodesSampleClass.MethodContainingIfStatementWithOnlyDiagnosticsNode)
                .GetFirstSyntaxNodeOfMethodFunc<ArgumentSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        private AridCheckResult GetAridCheckResult(Func<ClassDeclarationSyntax, SyntaxNode> getSyntaxNode)
        {
            var classDeclarationSyntax = SampleClassRelativePath
                .GetSampleClassDeclarationSyntax();
            var classification = Classifier.Classify(classDeclarationSyntax);
            var syntaxNode = getSyntaxNode(classDeclarationSyntax);
            return classification.GetResult(syntaxNode);
        }
    }
}
