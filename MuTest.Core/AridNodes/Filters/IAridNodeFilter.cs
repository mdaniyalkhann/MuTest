using MuTest.Core.Model.AridNodes;

namespace MuTest.Core.AridNodes.Filters
{
    public interface IAridNodeFilter
    {
        bool IsSatisfied(SimpleNode node);
    }
}