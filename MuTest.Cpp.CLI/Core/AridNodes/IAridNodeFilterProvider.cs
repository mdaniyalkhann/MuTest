using MuTest.Cpp.CLI.Core.AridNodes.Filters;

namespace MuTest.Cpp.CLI.Core.AridNodes
{
    public interface IAridNodeFilterProvider
    {
        IAridNodeFilter[] Filters { get; }
    }
}