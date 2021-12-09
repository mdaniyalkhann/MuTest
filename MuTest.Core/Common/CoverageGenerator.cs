using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static MuTest.Core.Common.Constants;

namespace MuTest.Core.Common
{
    public class CoverageGenerator : ICoverageGenerator
    {
        public ExecutionStatus CoverageStatus { get; private set; }

        public string HtmlCoveragePath { get; private set; } 

        public event EventHandler<string> OutputDataReceived;

        public async Task Generate(string xmlCoverage, string sourceDir)
        {
            HtmlCoveragePath = string.Empty;
            var coveragePath = xmlCoverage ?? throw new ArgumentNullException(nameof(xmlCoverage));
            var sourceDirPath = sourceDir ?? throw new ArgumentNullException(nameof(sourceDir));

            var targetPath = $"{Path.GetDirectoryName(xmlCoverage)}\\coverage";
            var projectBuilder = new StringBuilder($" -reports:{coveragePath}")
                .Append($" -sourcedirs:{sourceDirPath}")
                .Append($" -targetDir:{targetPath}")
                .Append(" -reporttypes:HtmlInline_AzurePipelines");

            if (!File.Exists("coverage\\ReportGenerator.exe"))
            {
                return;
            }

            try
            {
                var processInfo = new ProcessStartInfo("coverage\\ReportGenerator.exe")
                {
                    Arguments = $" {projectBuilder}",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                await Task.Run(() =>
                {
                    using (var process = new Process
                    {
                        StartInfo = processInfo,
                        EnableRaisingEvents = true
                    })
                    {
                        process.OutputDataReceived += ProcessOnOutputDataReceived;
                        process.ErrorDataReceived += ProcessOnOutputDataReceived;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        CoverageStatus = process.ExitCode == 0
                            ? ExecutionStatus.Success
                            : ExecutionStatus.Failed;
                        HtmlCoveragePath = Path.Combine(targetPath, "index.html");
                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                    }
                });
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to Build Product {0}", exp);
            }
        }

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(this, args.Data);
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnOutputDataReceived(args);
        }
    }
}