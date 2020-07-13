using MuTest.Core.Model;

namespace MuTest.Core.AridNodes.Filters
{
    public interface IAridNodeFilter
    {
        bool IsSatisfied(IAnalyzableNode node);
    }
}