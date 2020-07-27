using System.Text.RegularExpressions;
using MuTest.Core.AridNodes.Filters;

namespace MuTest.Cpp.CLI.Core.AridNodes.Filters
{
    public class ConsoleNodeFilter : IAridNodeFilter
    {
        private static readonly Regex ConsoleOutRegex = new Regex(@"cout\s*(<< .+)+");
        private static readonly Regex ConsoleInRegex = new Regex(@"cin\s*(>> .+)");
        private static readonly Regex ConsoleOutFunctionRegex = new Regex(@"cout\.\S+\(.+\)");
        private static readonly Regex ConsoleInFunctionRegex = new Regex(@"cin\.\S+\(.+\)");

        public bool IsSatisfied(string node)
        {
            return ConsoleOutRegex.IsMatch(node)
                || ConsoleInRegex.IsMatch(node)
                || ConsoleOutFunctionRegex.IsMatch(node)
                || ConsoleInFunctionRegex.IsMatch(node);
        }
    }
}