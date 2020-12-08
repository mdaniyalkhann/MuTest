using System.Text.RegularExpressions;

namespace MuTest.Cpp.CLI.Utility
{
    internal static class StringExtensions
    {
        public static string RemoveComments(this string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return line;
            }

            const string blockComments = @"\/\*(.*?)\*/";
            const string lineComments = @"\/\/.*";

            return Regex.Replace(line,
                $"{blockComments}|{lineComments}|",
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//"))
                    {
                        return string.Empty;
                    }

                    return me.Value;
                },
                RegexOptions.Singleline);
        }
    }
}
