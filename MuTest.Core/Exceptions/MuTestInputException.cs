using System;
using System.Text;
using MuTest.Core.Utility;

namespace MuTest.Core.Exceptions
{
    public class MuTestInputException : Exception
    {
        public string Details { get; }

        public MuTestInputException(string message, string details = "") : base(message)
        {
            Details = details.FixTrace();
        }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .AppendLine()
                .AppendLine()
                .AppendLine(Message)
                .AppendLine();

            if (!string.IsNullOrEmpty(Details))
            {
                builder.AppendLine(Details);
            }

            return builder.ToString();
        }
    }
}