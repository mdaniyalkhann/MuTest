using MuTest.Core.Model;

namespace MuTest.Core.AridNodes.Filters.HardCoded
{
    public class IONodeFilter : IAridNodeFilter
    {
        public bool IsSatisfied(IAnalyzableNode node)
        {
            var systemIoNamespace = typeof(System.IO.File).Namespace;
            return node.IsInvocationOfMemberOfTypeBelongingToNamespace(systemIoNamespace) ||
                   node.IsMemberAccessExpressionOfTypeBelongingToNamespace(systemIoNamespace);
        }
    }
}