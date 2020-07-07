using System.Diagnostics;
using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class DebugNodeFilter : IAridNodeFilter
    {
        public bool IsSatisfied(SimpleNode simpleNode)
        {
            return simpleNode.IsInvocationOfMemberOfType(typeof(Debug));
        }
    }
}