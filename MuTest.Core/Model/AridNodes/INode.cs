using Microsoft.CodeAnalysis;

namespace MuTest.Core.Model.AridNodes
{
    public interface INode
    {
        SyntaxNode SyntaxNode { get; }
    }
}