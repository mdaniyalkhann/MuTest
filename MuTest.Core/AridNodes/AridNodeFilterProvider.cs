using MuTest.Core.AridNodes.Filters;
using MuTest.Core.AridNodes.Filters.ByDefinition;
using MuTest.Core.AridNodes.Filters.HardCoded;

namespace MuTest.Core.AridNodes
{
    public class AridNodeFilterProvider : IAridNodeFilterProvider
    {
        public IAridNodeFilter[] Filters { get; } =
        {
            new ByDefinitionFilter(),
            new DebugNodeFilter(),
            new ConsoleNodeFilter(),
            new IONodeFilter()
        };
    }
}