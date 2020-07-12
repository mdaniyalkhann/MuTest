using Microsoft.CodeAnalysis;
using MuTest.Core.AridNodes;
using MuTest.Core.Model;
using MuTest.Core.Model.AridNodes;
using MuTest.Core.Model.ClassDeclarations;

namespace MuTest.Core.Common
{
    public class SyntaxNodeAnalysisFactory
    {
        private static readonly NodesClassifier NodesClassifier = new NodesClassifier();
        public SyntaxNodeAnalysis Create(SyntaxNode root, ClassDeclaration classDeclaration)
        {
            
            var analyzableNode = classDeclaration is ClassDeclarationWithSemantics classDeclarationWithSemantics
                ? new RoslynSyntaxNodeWithSemantics(root, classDeclarationWithSemantics.SemanticModel)
                : new RoslynSyntaxNode(root);
            var classification = NodesClassifier.Classify(analyzableNode);
            return new SyntaxNodeAnalysis(root, classification);
        }
    }
}