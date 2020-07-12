using System.Diagnostics;
using MuTest.Core.Model;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class DebugNodeFilter : IAridNodeFilter
    {
        public bool IsSatisfied(IAnalyzableNode simpleNode)
        {
            return simpleNode.IsInvocationOfMemberOfType(typeof(Debug)) ||
                   simpleNode.IsMemberAccessExpressionOfType(typeof(Debug));
        }
    }
}