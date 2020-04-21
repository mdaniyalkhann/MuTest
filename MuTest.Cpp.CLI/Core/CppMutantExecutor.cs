using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Mutants;
using MuTest.Core.Utility;
using MuTest.Cpp.CLI.Model;
using MuTest.Cpp.CLI.Mutants;
using MuTest.Cpp.CLI.Utility;

namespace MuTest.Cpp.CLI.Core
{
    public class CppMutantExecutor
    {
        public double SurvivedThreshold { get; set; } = 1;

        public double KilledThreshold { get; set; } = 1;

        public bool CancelMutationOperation { get; set; }

        public bool EnableDiagnostics { get; set; }

        private readonly CppClass _cpp;
        private readonly CppBuildContext _context;
        private readonly VSTestConsoleSettings _settings;

        private string _testDiagnostics;

        private string _buildDiagnostics;

        public event EventHandler<CppMutantEventArgs> MutantExecuted;

        public virtual void OnMutantExecuted(CppMutantEventArgs args)
        {
            MutantExecuted?.Invoke(this, args);
        }

        public CppMutantExecutor(CppClass cpp, CppBuildContext context, VSTestConsoleSettings settings)
        {
            _cpp = cpp ?? throw new ArgumentNullException(nameof(cpp));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public string LastExecutionOutput { get; set; }

        public int NumberOfMutantsExecutingInParallel { get; set; }

        public async Task ExecuteMutants()
        {
            var mutationProcessLog = new StringBuilder();
            if (!_cpp.Mutants.Any())
            {
                PrintMutationReport(mutationProcessLog, _cpp.Mutants);
                return;
            }

            var mutants = _cpp.NotRunMutants;

            var testTasks = new List<Task>();
            int totalMutants = mutants.Count;
            for (var index = 0; index < mutants.Count; index += NumberOfMutantsExecutingInParallel)
            {
                if (CancelMutationOperation)
                {
                    break;
                }

                var directoryIndex = -1;
                for (var mutationIndex = index; mutationIndex < Math.Min(index + NumberOfMutantsExecutingInParallel, mutants.Count); mutationIndex++)
                {
                    directoryIndex++;
                    var mutant = mutants[mutationIndex];
                    _cpp.SourceClass.ReplaceLine(
                        mutant.Mutation.LineNumber,
                        mutant.Mutation.ReplacementNode,
                        _context.TestContexts[directoryIndex].SourceClass.FullName);
                }

                var buildExecutor = new CppBuildExecutor(
                    _settings, 
                    _context.TestSolution.FullName, 
                    Path.GetFileNameWithoutExtension(_cpp.TestProject))
                {
                    EnableLogging = false,
                    Configuration = _cpp.Configuration,
                    Platform = _cpp.Platform,
                    IntDir = _context.IntDir,
                    OutDir = _context.OutDir,
                    OutputPath = _context.OutputPath,
                    IntermediateOutputPath = _context.IntermediateOutputPath
                };

                await buildExecutor.ExecuteBuild();

                var log = new StringBuilder();
                void OutputDataReceived(object sender, string args) => log.Append(args.PrintWithPreTag());

                if (buildExecutor.LastBuildStatus == Constants.BuildExecutionStatus.Failed)
                {
                    if (EnableDiagnostics)
                    {
                        log.AppendLine("<fieldset style=\"margin-bottom:10\">");
                        buildExecutor.OutputDataReceived += OutputDataReceived;

                        log.AppendLine("</fieldset>");
                        _buildDiagnostics = log.ToString();
                    }

                    buildExecutor.OutputDataReceived -= OutputDataReceived;
                }

                directoryIndex = -1;
                for (var mutationIndex = index; mutationIndex < Math.Min(index + NumberOfMutantsExecutingInParallel, mutants.Count); mutationIndex++)
                {
                    if (CancelMutationOperation)
                    {
                        break;
                    }

                    if (decimal.Divide(mutants.Count(x => x.ResultStatus == MutantStatus.Survived), totalMutants) > (decimal)SurvivedThreshold ||
                        decimal.Divide(mutants.Count(x => x.ResultStatus == MutantStatus.Killed), totalMutants) > (decimal)KilledThreshold)
                    {
                        break;
                    }

                    var mutant = mutants[mutationIndex];

                    try
                    {
                        if (buildExecutor.LastBuildStatus == Constants.BuildExecutionStatus.Failed)
                        {
                            mutant.ResultStatus = MutantStatus.BuildError;
                            OnMutantExecuted(new CppMutantEventArgs
                            {
                                Mutant = mutant,
                                TestLog = _testDiagnostics,
                                BuildLog = _buildDiagnostics
                            });
                            continue;
                        }

                        directoryIndex++;
                        var current = directoryIndex;
                        testTasks.Add(Task.Run(() => ExecuteTests(mutant, current)));
                    }
                    catch (Exception e)
                    {
                        mutant.ResultStatus = MutantStatus.Skipped;
                        Trace.TraceError("Unable to Execute Mutant {0} Exception: {1}", mutant.Mutation.OriginalNode.Encode(), e);
                    }
                }

                await Task.WhenAll(testTasks);
            }

            PrintMutationReport(mutationProcessLog, mutants);
        }

        private void PrintMutationReport(StringBuilder mutationProcessLog, IList<CppMutant> mutants)
        {
            mutationProcessLog.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
            mutationProcessLog.AppendLine("Mutation Report".PrintImportantWithLegend());
            mutationProcessLog.Append("  ".PrintWithPreTag());
            mutationProcessLog.Append($"{"Source Path   :".PrintImportant()} {_cpp.SourceClass}".PrintWithPreTag());
            mutationProcessLog.Append($"{"Test Path     :".PrintImportant()} {_cpp.TestClass}".PrintWithPreTag());
            mutationProcessLog.Append($"{"Test Project  :".PrintImportant()} {_cpp.TestProject}".PrintWithPreTag());
            mutationProcessLog.Append("  ".PrintWithPreTag());

            if (mutants.Any())
            {
                mutationProcessLog.AppendLine("<fieldset style=\"margin-bottom:10\">");
                mutationProcessLog.Append("Mutants".PrintImportantWithLegend(color: Constants.Colors.BlueViolet));
            }

            foreach (var mutant in mutants)
            {
                var lineNumber = mutant.Mutation.LineNumber;
                mutationProcessLog
                    .Append(
                        $"Line: {lineNumber.ToString().PrintImportant(color: Constants.Colors.Blue)} - {mutant.ResultStatus.ToString().PrintImportant()} - {mutant.Mutation.DisplayName.Encode()}"
                            .PrintWithDateTime()
                            .PrintWithPreTagWithMargin());
            }

            if (mutants.Any())
            {
                mutationProcessLog.AppendLine("</fieldset>");
            }

            mutationProcessLog.AppendLine("</fieldset>");

            LastExecutionOutput = mutationProcessLog.ToString();
        }

        private async Task ExecuteTests(CppMutant mutant, int index)
        {
            var testExecutor = new GoogleTestExecutor
            {
                KillProcessOnTestFail = true
            };

            var log = new StringBuilder();
            void OutputDataReceived(object sender, string args) => log.Append(args.PrintWithPreTag());
            if (EnableDiagnostics)
            {
                log.AppendLine("<fieldset style=\"margin-bottom:10\">");
                var lineNumber = mutant.Mutation.LineNumber;
                log.Append(
                    $"Line: {lineNumber.ToString().PrintImportant(color: Constants.Colors.Blue)} - {mutant.Mutation.DisplayName.Encode()}"
                        .PrintWithDateTime()
                        .PrintWithPreTag());
                testExecutor.OutputDataReceived += OutputDataReceived;
            }

            var projectDirectory = Path.GetDirectoryName(_cpp.TestProject);
            var projectName = Path.GetFileNameWithoutExtension(_cpp.TestProject);

            await testExecutor.ExecuteTests(
                $"{projectDirectory}/{_context.OutDir}{projectName}.exe",
                $"{Path.GetFileNameWithoutExtension(_context.TestContexts[index].TestClass.Name)}.*");

            testExecutor.OutputDataReceived -= OutputDataReceived;
            if (EnableDiagnostics && testExecutor.LastTestExecutionStatus == Constants.TestExecutionStatus.Timeout)
            {
                log.AppendLine("</fieldset>");
                _testDiagnostics = log.ToString();
            }

            mutant.ResultStatus = testExecutor.LastTestExecutionStatus == Constants.TestExecutionStatus.Success
                ? MutantStatus.Survived
                : testExecutor.LastTestExecutionStatus == Constants.TestExecutionStatus.Timeout
                    ? MutantStatus.Timeout
                    : MutantStatus.Killed;
            OnMutantExecuted(new CppMutantEventArgs
            {
                Mutant = mutant,
                BuildLog = _buildDiagnostics,
                TestLog = _testDiagnostics
            });
        }

        public void PrintMutatorSummary(StringBuilder mutationProcessLog, IList<CppMutant> mutants)
        {
            var mutators = mutants
                .GroupBy(grp => grp.Mutation.Type)
                .Select(x => new
                {
                    Mutator = x.Key,
                    Mutants = x.ToList()
                });
            mutationProcessLog.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
            mutationProcessLog.AppendLine("MutatorWise Summary".PrintImportantWithLegend());
            foreach (var mutator in mutators)
            {
                var survived = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.Survived);
                var killed = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.Killed);
                var uncovered = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.NotCovered);
                var timeout = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.Timeout);
                var buildErrors = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.BuildError);
                var skipped = mutator.Mutants.Count(x => x.ResultStatus == MutantStatus.Skipped);
                var covered = mutator.Mutants.Count(x => x.ResultStatus != MutantStatus.NotCovered) - timeout - buildErrors - skipped;
                var coverage = decimal.Divide(killed, covered == 0
                    ? 1
                    : covered);
                var mutation = covered == 0
                    ? "N/A"
                    : $"{killed}/{covered}[{coverage:P}]";
                mutationProcessLog.AppendLine("<fieldset>");
                mutationProcessLog.AppendLine($"{mutator.Mutator}".PrintImportantWithLegend());
                mutationProcessLog.Append($"Coverage: Mutation({mutation}) [Survived({survived}) Killed({killed}) Not Covered({uncovered}) Timeout({timeout}) Build Errors({buildErrors}) Skipped({skipped})]"
                    .PrintWithPreTagWithMarginImportant(color: Constants.Colors.Blue));
                mutationProcessLog.AppendLine("</fieldset>");
            }

            mutationProcessLog.AppendLine("</fieldset>");
        }

        public void PrintClassSummary(CppClass cppClass, StringBuilder mutationProcessLog)
        {
            cppClass.CalculateMutationScore();
            mutationProcessLog.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10\">");
            mutationProcessLog.AppendLine("ClassWise Summary".PrintImportantWithLegend());
            mutationProcessLog.Append(cppClass.MutationScore.ToString().PrintWithPreTagWithMarginImportant(color: Constants.Colors.BlueViolet));
            mutationProcessLog.Append(
                $"Coverage: Mutation({cppClass.MutationScore.Mutation})"
                    .PrintWithPreTagWithMarginImportant(color: Constants.Colors.Blue));
            mutationProcessLog.AppendLine("</fieldset>");
        }
    }
}
