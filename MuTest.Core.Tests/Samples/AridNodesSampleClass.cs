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
            System.Diagnostics.Debug.Assert(true);
        }

        public void MethodContainingSingleConsoleNode()
        {
            Console.WriteLine("Hello World");
        }

        public void MethodContainingLoopWithOnlyDiagnosticsNode()
        {
            for (;;)
            {
                System.Diagnostics.Debug.Print("Ok");
            }
        }

        public void MethodContainingIfStatementWithOnlyDiagnosticsNode(int p)
        {
            if (p > 10)
            {
                System.Diagnostics.Debug.Print("Ok");
            }
            else
            {
                System.Diagnostics.Debug.Fail("NotValid");
            }
        }

        public bool ContainsOkText(string input)
        {
            return input.Contains("Ok");
        }

        public void MethodContainingNonDiagnosticsNodeWithSameNameAsDiagnosticsDebug()
        {
            Debug.Print("Test");
        }
    }

    internal static class Debug
    {
        public static void Print(string text)
        {
            // Do something
        }
    }
}