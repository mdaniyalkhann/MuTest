using System;

namespace MuTest.Core.Common
{
    public static class HtmlHelper
    {
        private const string DefaultColor = "#333";

        public static string PrintWithDateTime(this string input)
        {
            return $"{input} - {DateTime.Now.ToString("s").PrintImportant()}";
        }

        public static string PrintWithDateTimeSimple(this string input)
        {
            return $"{input} - {DateTime.Now:s}";
        }

        public static string Print(this string text, double fontSize = 2, string color = DefaultColor)
        {
            return $@"<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}"">{text}</font>";
        }

        public static string PrintImportant(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}""><strong>{text}</strong></font>";
        }

        public static string PrintImportantWithLegend(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"<legend><font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}""><strong>{text}</strong></font></legend>";
        }

        public static string PrintWithPreTag(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"{Constants.PreStart}<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}"">{text}</font>{Constants.PreEnd}";
        }

        public static string PrintWithPreTagImportant(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"{Constants.PreStart}<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}""><strong>{text}</strong></font>{Constants.PreEnd}";
        }

        public static string PrintWithPreTagWithMargin(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"{Constants.PreStartWithMargin}<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}"">{text}</font>{Constants.PreEnd}";
        }

        public static string PrintWithPreTagWithMarginImportant(this string text, int fontSize = 2, string color = DefaultColor)
        {
            return $@"{Constants.PreStartWithMargin}<font style=""font-family:consolas;"" size=""{fontSize}"" color=""{color}""><strong>{text}</strong></font>{Constants.PreEnd}";
        }
    }
}