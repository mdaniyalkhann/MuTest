using System;

namespace MuTest.Core.Model
{
    public class CodeAnalysisProjectLoadException : Exception
    {
        public CodeAnalysisProjectLoadException(string message) : base(message)
        {
        }
    }
}