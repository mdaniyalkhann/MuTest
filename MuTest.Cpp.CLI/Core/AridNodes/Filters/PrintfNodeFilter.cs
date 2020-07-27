using System.Text.RegularExpressions;

namespace MuTest.Cpp.CLI.Core.AridNodes.Filters
{
    public class PrintfNodeFilter : IAridNodeFilter
    {
        private static readonly Regex PrintfRegex =
            new Regex(@"(printf|fprintf|sprintf|snprintf|printf_s|fprintf_s|sprintf_s|snprintf_s)\s*\(.+\)");

        public bool IsSatisfied(string node)
        {
            return PrintfRegex.IsMatch(node);
        }
    }
}