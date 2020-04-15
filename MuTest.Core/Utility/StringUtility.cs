using System;
using System.Linq;

namespace MuTest.Core.Utility
{
    public static class StringUtility
    {
        /// <summary>
        /// Get string value between [first] a and [last] b.
        /// </summary>
        public static string Between(this string value, string a, string b)
        {
            int posA = value.IndexOf(a, StringComparison.InvariantCulture);
            int posB = value.LastIndexOf(b, StringComparison.InvariantCulture);
            if (posA == -1)
            {
                return "";
            }
            if (posB == -1)
            {
                return "";
            }
            int adjustedPosA = posA + a.Length;
            if (adjustedPosA >= posB)
            {
                return "";
            }
            return value.Substring(adjustedPosA, posB - adjustedPosA);
        }

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