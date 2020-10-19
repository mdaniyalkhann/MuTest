using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Serilog;
using Log4NetLogManager = log4net.LogManager;
using NLogLogManager = NLog.LogManager;

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
        public void MethodContainingSingleIONode()
        {
            File.ReadAllText("SamplePath");
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

        public void MethodContainingLog4NetNode()
        {
            var log = Log4NetLogManager.GetLogger(GetType());
            log.Debug("Test");
        }

        public void MethodContainingNLogNode()
        {
            var log = NLogLogManager.GetCurrentClassLogger();
            log.Debug("Test");
        }

        public void MethodContainingSerilogNode()
        {
            Log.Debug("Test");
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