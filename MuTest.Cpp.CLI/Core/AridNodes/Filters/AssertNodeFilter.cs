using System.Text.RegularExpressions;

namespace MuTest.Cpp.CLI.Core.AridNodes.Filters
{
    public class AssertNodeFilter : IAridNodeFilter
    {
        private static readonly Regex AssertRegex = new Regex(@"(assert|cassert|static_assert)\s*\(.+\)");
        public bool IsSatisfied(string node)
        {
            return AssertRegex.IsMatch(node);
        }
    }
}