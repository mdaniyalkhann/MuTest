using System.Text.RegularExpressions;

namespace MuTest.Cpp.CLI.Core.AridNodes.Filters
{
    public class MemoryAllocationNodeFilter : IAridNodeFilter
    {
        private static readonly Regex MallocRegex = new Regex(@"(malloc|calloc|realloc|free)\s*\(.+\)");
        public bool IsSatisfied(string node)
        {
            return MallocRegex.IsMatch(node);
        }
    }
}