using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable UnusedMember.Global

namespace MuTest.Core.Tests.Samples
{
    [ExcludeFromCodeCoverage]
    public class AridNodesSampleClass
    {
        public int MethodContainingSingleBinaryExpression(int x)
        {
            return x + 1;
        }

        public void MethodContainingSingleDiagnosticsNode()
        {
            Debug.Assert(true);
        }

        public void MethodContainingSingleConsoleNode()
        {
            Console.WriteLine("Hello World");
        }

        public void MethodContainingLoopWithOnlyDiagnosticsNode()
        {
            for (;;)
            {
                Debug.Print("Ok");
            }
        }

        public void MethodContainingIfStatementWithOnlyDiagnosticsNode(int p)
        {
            if (p > 10)
            {
                Debug.Print("Ok");
            }
            else
            {
                Debug.Fail("NotValid");
            }
        }

        public bool ContainsOkText(string input)
        {
            return input.Contains("Ok");
        }
    }
}