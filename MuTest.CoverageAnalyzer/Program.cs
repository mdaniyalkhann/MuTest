using System;
using System.Diagnostics;
using MuTest.Core.Exceptions;
using MuTest.Core.Testing;

namespace MuTest.CoverageAnalyzer
{
    public class Program
    {
        private static IChalk _chalk;

        private static int Main(string[] args)
        {
            try
            {
                Trace.Listeners.Add(new EventLogTraceListener("MuTest_Coverage_Analyzer"));

                _chalk = new Chalk();
                var coverageAnalyzer = new CoverageAnalyzerRunner();
                var app = new CoverageAnalyzerCli(coverageAnalyzer);
                return app.Run(args);
            }
            catch (MuTestInputException strEx)
            {
                ShowMessage(strEx);
                return 1;
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException.GetType() == typeof(MuTestInputException))
                {
                    ShowMessage(ex.InnerException);
                    return 1;
                }

                if(ex.Message.StartsWith("Unrecognized option"))
                {
                    _chalk.Default($"{ex.Message}{Environment.NewLine}");
                    return 1;
                }

                Trace.TraceError("Exception suppressed: {0}", ex);
                ShowMessage(ex);
                return 1;
            }
        }

        private static void ShowMessage(Exception strEx)
        {
            _chalk.Yellow("Coverage Analyzer C# failed to analyze repository. For more information see the logs below:");
            _chalk.Default(strEx.ToString());
        }
    }
}