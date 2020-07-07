using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model.AridNodes
{
    public class CompoundNode : INode
    {
        public CompoundNode(SyntaxNode syntaxNode)
        {
            SyntaxNode = syntaxNode;
        }

        public SyntaxNode SyntaxNode { get; }
    }
}