using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MuTest.Core.AridNodes;
using MuTest.Core.Common.ClassDeclarationLoaders;
using MuTest.Core.Model;
using MuTest.Core.Model.AridNodes;
using MuTest.Core.Tests.Samples;
using MuTest.Core.Tests.Utility;
using NUnit.Framework;
using Shouldly;

namespace MuTest.Core.Tests
{
    /// <summary>
    /// <see cref="NodesClassifier"/>
    /// </summary>
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class NodesClassifierTests
    {
        private const string SampleClassRelativePath = @"Samples\AridNodesSampleClass.cs";
        private static readonly NodesClassifier Classifier = new NodesClassifier();

        [Test]
        public void Check_WhenNodeIsSimpleBinaryExpression_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingSingleBinaryExpression);
            var getSyntaxNode = methodName
                .GetFirstSyntaxNodeOfMethodFunc<BinaryExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeFalse(),
                () => result.TriggeredBy.ShouldBeEmpty());
        }

        [Test]
        public void Check_WhenNodeIsDebugNode_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingSingleDiagnosticsNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<InvocationExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenNodeIsNodeNamedDebugButNoDiagnostics_ShouldNotBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingNonDiagnosticsNodeWithSameNameAsDiagnosticsDebug);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<InvocationExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeFalse(),
                () => result.TriggeredBy.ShouldBeEmpty());
        }

        [Test]
        public void Check_WhenLiteralIsArgumentOfDebugNode_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingSingleDiagnosticsNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<LiteralExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenNodeIsConsoleNode_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingSingleConsoleNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<InvocationExpressionSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_WhenLoopStatementIsOnlyContainingDebugStatements_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingLoopWithOnlyDiagnosticsNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<ForStatementSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        [Test]
        public void Check_IfStatementIsOnlyContainingDebugStatements_ShouldNotBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingIfStatementWithOnlyDiagnosticsNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<IfStatementSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeFalse(),
                () => result.TriggeredBy.ShouldBeEmpty());
        }

        [Test]
        public void Check_LiteralArgumentOfMethod_ShouldBeArid()
        {
            // Arrange
            const string methodName = nameof(AridNodesSampleClass.MethodContainingIfStatementWithOnlyDiagnosticsNode);
            var getSyntaxNode = methodName.GetFirstSyntaxNodeOfMethodFunc<ArgumentSyntax>();

            // Act
            var result = GetAridCheckResult(getSyntaxNode, methodName);

            // Assert
            result.ShouldSatisfyAllConditions(
                () => result.IsArid.ShouldBeTrue(),
                () => result.TriggeredBy.ShouldNotBeEmpty());
        }

        private AridCheckResult GetAridCheckResult(Func<ClassDeclarationSyntax, SyntaxNode> getSyntaxNode, string methodName)
        {
            var sampleProjectAbsolutePath = SyntaxExtension.GetSampleProjectAbsolutePath();
            var sampleClassAbsolutePath = SampleClassRelativePath.GetSampleClassAbsoluteFilePath();
            var semanticsClassDeclarationLoader = new SemanticsClassDeclarationLoader();
            var classDeclarationWithSemantics = semanticsClassDeclarationLoader.Load(sampleClassAbsolutePath, sampleProjectAbsolutePath,
                nameof(AridNodesSampleClass));

            var classDeclarationSyntax = classDeclarationWithSemantics.Syntax;
            var methodDeclaration = classDeclarationSyntax.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.Text == methodName);
            var syntaxNode = getSyntaxNode(classDeclarationSyntax);
            var analysisRoot =
                new RoslynSyntaxNodeWithSemantics(methodDeclaration, classDeclarationWithSemantics.SemanticModel);
            var classification = Classifier.Classify(analysisRoot);
            var node = new RoslynSyntaxNode(syntaxNode);
            return classification.GetResult(node);
        }
    }
}
