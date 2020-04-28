using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common.Settings;
using static MuTest.Core.Common.Constants;

namespace MuTest.Cpp.CLI.Core
{
    public class CppBuildExecutor
    {
        private readonly MuTestSettings _settings;
        private readonly string _solution;
        private readonly string _project;

        public event EventHandler<EventArgs> BuildStarted;
        public event EventHandler<EventArgs> BuildFinished;
        public event EventHandler<string> OutputDataReceived;

        public bool EnableLogging { get; set; } = true;

        public string OutputPath { get; set; } = string.Empty;

        public string IntermediateOutputPath { get; set; } = string.Empty;

        public string OutDir { get; set; } = string.Empty;

        public string IntDir { get; set; } = string.Empty;

        public string Platform { get; set; } = string.Empty;

        public string Configuration { get; set; } = string.Empty;

        public BuildExecutionStatus LastBuildStatus { get; private set; }

        public bool QuietWithSymbols { get; set; }

        public CppBuildExecutor(MuTestSettings settings, string solution, string project)
        {
            if (string.IsNullOrWhiteSpace(project))
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (string.IsNullOrWhiteSpace(solution))
            {
                throw new ArgumentNullException(nameof(solution));
            }

            _project = project;
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _solution = solution;
        }

        public async Task ExecuteBuildWithoutDependencies()
        {
            var projectBuilder = new StringBuilder($@"""{_solution}""")
                .Append($" -t:\"{_project}\"")
                .Append(_settings.MSBuildDependenciesOption);

            await Build(projectBuilder);
        }

        public async Task ExecuteBuild()
        {
            await Build(new StringBuilder($@"""{_solution}""")
                .Append($" -t:\"{_project}\""));
        }

        protected virtual void OnBuildStarted(EventArgs args)
        {
            BuildStarted?.Invoke(this, args);
        }

        protected virtual void OnBuildFinished(EventArgs args)
        {
            BuildFinished?.Invoke(this, args);
        }

        protected virtual void OnOutputDataReceived(DataReceivedEventArgs args)
        {
            OutputDataReceived?.Invoke(this, args.Data);
        }

        private async Task Build(StringBuilder projectBuilder)
        {
            OnBuildStarted(EventArgs.Empty);
            projectBuilder
                .Append(_settings.PostBuildEvents)
                .Append(_settings.PreBuildEvents)
                .Append(_settings.MSBuildCustomOption);

            if (EnableLogging)
            {
                var buildLogFilePath = $@"""{_settings.MSBuildLogDirectory}build_{DateTime.Now:yyyyMdhhmmss}.log""";
                projectBuilder.Append(_settings.MSBuildLogger)
                    .Append(buildLogFilePath)
                    .Append(";")
                    .Append(VerbosityOption)
                    .Append(_settings.MSBuildVerbosity);
            }
            else if (QuietWithSymbols)
            {
                projectBuilder
                    .Append(VerbosityOption)
                    .Append(_settings.QuietBuildWithSymbols);
            }
            else
            {
                projectBuilder
                    .Append(VerbosityOption)
                    .Append(_settings.QuietBuild);
            }

            if (!string.IsNullOrWhiteSpace(OutputPath))
            {
                projectBuilder.Append($" /p:OutputPath=\"{OutputPath}\"");
            }

            if (!string.IsNullOrWhiteSpace(IntermediateOutputPath))
            {
                projectBuilder.Append($" /p:IntermediateOutputPath=\"{IntermediateOutputPath}\"");
            }

            if (!string.IsNullOrWhiteSpace(OutDir))
            {
                projectBuilder.Append($" /p:OutDir=\"{OutDir}\"");
            }


            if (!string.IsNullOrWhiteSpace(IntDir))
            {
                projectBuilder.Append($" /p:IntDir=\"{IntDir}\"");
            }

            if (!string.IsNullOrWhiteSpace(Platform))
            {
                projectBuilder.Append($" /p:Platform=\"{Platform}\"");
            }

            if (!string.IsNullOrWhiteSpace(Configuration))
            {
                projectBuilder.Append($" /p:Configuration=\"{Configuration}\"");
            }

            try
            {
                var processInfo = new ProcessStartInfo(_settings.MSBuildPath)
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

                        LastBuildStatus = process.ExitCode == 0
                            ? BuildExecutionStatus.Success
                            : BuildExecutionStatus.Failed;

                        OnBuildFinished(EventArgs.Empty);
                        process.OutputDataReceived -= ProcessOnOutputDataReceived;
                        process.ErrorDataReceived -= ProcessOnOutputDataReceived;
                    }
                });
            }
            catch (Exception exp)
            {
                LastBuildStatus = BuildExecutionStatus.Failed;
                Trace.TraceError("Unable to Build Product {0}", exp);
            }
        }

        private void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
        {
            OnOutputDataReceived(args);
        }
    }
}