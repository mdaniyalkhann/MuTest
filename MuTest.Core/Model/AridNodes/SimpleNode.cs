using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model.AridNodes
{
    public class SimpleNode : INode
    {
        public SimpleNode(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
        }

        public SyntaxNode SyntaxNode { get; }
    }
}