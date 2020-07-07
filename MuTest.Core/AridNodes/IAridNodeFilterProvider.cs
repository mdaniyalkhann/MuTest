using MuTest.Core.AridNodes.Filters;

namespace MuTest.Core.AridNodes
{
    public interface IAridNodeFilterProvider
    {
        IAridNodeFilter[] Filters { get; }
    }
}