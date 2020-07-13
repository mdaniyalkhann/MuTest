using System;
using System.Collections.Generic;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.Model
{
    public interface IAnalyzableNode
    {
        IEnumerable<IAnalyzableNode> ChildNodes();
        IEnumerable<IAnalyzableNode> DescendantNodesAndSelf();
        IEnumerable<IAnalyzableNode> DescendantNodes();
        bool IsCompoundNode { get; }
        AnalyzableNodeSyntaxType SyntaxType { get; }
        IAnalyzableNode Parent { get; }
        bool IsInvocationOfMemberOfType(Type type);
        string Kind();
        string GetText();
        bool IsMemberAccessExpressionOfType(Type type);
        bool IsInvocationOfMemberOfTypeBelongingToNamespace(string @namespace);
        bool IsMemberAccessExpressionOfTypeBelongingToNamespace(string @namespace);
    }
}