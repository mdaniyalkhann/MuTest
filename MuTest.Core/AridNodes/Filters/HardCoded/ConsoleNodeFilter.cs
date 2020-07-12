using System;
using MuTest.Core.Model;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class ConsoleNodeFilter : IAridNodeFilter
    {
        public bool IsSatisfied(IAnalyzableNode simpleNode)
        {
            return simpleNode.IsInvocationOfMemberOfType(typeof(Console)) ||
                   simpleNode.IsMemberAccessExpressionOfType(typeof(Console));
        }
    }
}