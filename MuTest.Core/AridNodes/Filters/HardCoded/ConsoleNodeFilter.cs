using System;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class ConsoleNodeFilter : IAridNodeFilter
    {
        public bool IsSatisfied(SimpleNode simpleNode)
        {
            return simpleNode.IsInvocationOfMemberOfType(typeof(Console));
        }
    }
}