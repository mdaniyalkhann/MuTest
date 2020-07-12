using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model.AridNodes
{
    public class SyntaxNodeAnalysis
    {
        private readonly NodesClassification _nodesClassification;

        public SyntaxNodeAnalysis(SyntaxNode root, NodesClassification nodesClassification)
        {
            Root = root;
            _nodesClassification = nodesClassification;
        }

        public SyntaxNode Root { get; }

        public bool IsNodeArid(SyntaxNode syntaxNode)
        {
            var analyzableNode = new RoslynSyntaxNode(syntaxNode);
            return _nodesClassification.GetResult(analyzableNode).IsArid;
        }
    }
}