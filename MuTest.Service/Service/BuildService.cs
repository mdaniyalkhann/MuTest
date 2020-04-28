using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model.Service;
using static MuTest.Core.Common.Constants;

namespace MuTest.Service.Service
{
    internal class BuildService
    {
        private readonly MuTestSettings _settings;
        private StringBuilder _buildOutput;

        public BuildService(MuTestSettings settings)
        {
            _settings = settings;
        }

        public async Task<BuildResult> Build(string options)
        {
            var result = new BuildResult();
            try
            {
                var processInfo = new ProcessStartInfo(_settings.MSBuildPath)
                {
                    Arguments = $" {options}",
                    UseShellExecute = false,
                    CreateNoWindow = false,
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
                        _buildOutput = new StringBuilder();
                        process.OutputDataReceived += ProcessOnOutputDataReceived;
                        process.ErrorDataReceived += ProcessOnOutputDataReceived;
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();

                        result.Status = process.ExitCode == 0
                            ? BuildExecutionStatus.Success
                            : BuildExecutionStatus.Failed;

                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                    }
                });
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to Build Product {0}", exp);
                result.Status = BuildExecutionStatus.Failed;
            }
            finally
            {
                result.BuildOutput = _buildOutput?.ToString();
            }

            return result;
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _buildOutput.Append($"{e.Data}\n");
        }
    }
}
