using MuTest.Cpp.CLI.Core.AridNodes.Filters;

namespace MuTest.Cpp.CLI.Core.AridNodes
{
    public class AridNodeFilterProvider : IAridNodeFilterProvider
    {
        public IAridNodeFilter[] Filters { get; } =
        {
            new ConsoleNodeFilter(),
            new PrintfNodeFilter(),
            new AssertNodeFilter(),
            new MemoryAllocationNodeFilter()
        };
    }
}