using System.Linq;

namespace MuTest.Core.Utility
{
    public static class StringUtility
    {
        public static string FixTrace(this string str)
        {
            var numberOfCharactersToSkip = str.Length;
            var traceLimit = 32766;
            while (numberOfCharactersToSkip > traceLimit)
            {
                numberOfCharactersToSkip -= 10000;
            }

            return new string(str.Skip(str.Length - numberOfCharactersToSkip).ToArray());
        }
    }
}