using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Dashboard.Common;
using Dashboard.Views;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using MuTest.Core.Common;
using MuTest.Core.Common.Settings;
using MuTest.Core.Model;
using MuTest.Core.Mutants;
using MuTest.Core.Mutators;
using MuTest.Core.Testing;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class MutantViewerViewModel
    {
        private static readonly MuTestSettings Settings = MuTestSettingsSection.GetSettings();

        public virtual bool IsSplashScreenShown { get; set; }

        [ServiceProperty(Key = "MessageBoxService")]
        protected virtual IMessageBoxService MessageBoxService => null;

        private int _numberOfMutantsExecutingInParallel;

        [ServiceProperty(SearchMode = ServiceSearchMode.PreferParents, Key = "MutationService")]
        protected virtual IDocumentManagerService MutationDocumentManagerService => null;

        [ServiceProperty(Key = "NotificationService")]
        protected virtual INotificationService NotificationService => null;

        public virtual ListBoxEditViewModel MutantList { get; }

        public virtual bool MutantOperationsEnabled { get; set; } = true;

        public virtual Visibility ProgressBarMutationVisible { get; set; } = Visibility.Hidden;

        public virtual string DisplayFormat => ProgressBarFormat;

        public virtual double CurrentProgress { get; set; }

        public virtual double MaximumProgress { get; set; }

        public virtual double MinimumProgress { get; set; }

        public ControlViewModel ChkExecuteAllTests { get; }

        public ControlViewModel ChkAnalyzeExternalCoverage { get; }

        public ControlViewModel ChkUseClassFilter { get; }

        public ControlViewModel ChkRealTimeAnalysis { get; }

        public ControlViewModel ChkEnableDiagnostic { get; }

        public ControlViewModel ChkEnableCodeCoverage { get; }

        public ControlViewModel ChkOptimizeTestProject { get; }

        public virtual decimal NumberOfMutantsExecutedInParallel { get; set; } = 5;

        public virtual decimal MutantsPerLine { get; set; } = 1;

        public virtual HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;

        public virtual VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

        public virtual string MutantFilterRegEx { get; set; }

        public virtual string SpecificMutantRegEx { get; set; }

        public virtual string MutantFilterId { get; set; }

        public virtual string BuildExtensions { get; set; } = ".dll,.exe";

        public virtual List<IMutator> SelectedMutators { get; } = new List<IMutator>();

        public virtual ObservableCollection<MutantDetail> MutantsDetails { get; } = new ObservableCollection<MutantDetail>();

        public virtual ObservableCollection<MutantDetail> SelectedMutants { get; } = new ObservableCollection<MutantDetail>();

        public virtual bool DisableMutators { get; set; }

        private readonly SourceClassDetail _source;
        private readonly ICommandPromptOutputLogger _outputLogger;
        private readonly ITestDirectoryFactory _directoryFactory;
        private readonly IMutantInitializer _mutantInitializer;
        private IMutantExecutor _mutantExecutor;
        private readonly CommandPromptOutputViewerViewModel _testDiagnosticDocumentViewModel;
        private readonly CommandPromptOutputViewerViewModel _buildDiagnosticDocumentViewModel;
        private IDocument _testDiagnosticDocument;
        private IDocument _buildDiagnosticDocument;

        private readonly IDictionary<string, MutantStatus> _previousMutants;
        private readonly bool _isRealMutationAnalysisRunning;
        private object _selectedMutators;
        private FileSystemWatcher _watcher;
        private bool _idle;
        private bool _silently;
        private IDocument _printDocument;
        private StringBuilder _mutationProcessLog;
        private bool _isClosed;
        private bool _testsAreFailing;

        protected MutantViewerViewModel()
        {
        }

        protected MutantViewerViewModel(SourceClassDetail source)
        {
            _source = source;
            _silently = false;
            MutantList = ListBoxEditViewModel.CreateListBoxEdit();
            _outputLogger = new CommandPromptOutputLogger();
            ChkExecuteAllTests = ControlViewModel.Create();
            ChkEnableDiagnostic = ControlViewModel.Create();
            ChkEnableCodeCoverage = ControlViewModel.CreateWithChecked();
            ChkAnalyzeExternalCoverage = ControlViewModel.Create();
            ChkOptimizeTestProject = ControlViewModel.CreateWithChecked();
            ChkUseClassFilter = ControlViewModel.CreateWithChecked();
            ChkRealTimeAnalysis = ControlViewModel.Create();
            _testDiagnosticDocumentViewModel = _outputLogger.GetLogFromOutput("Test Diagnostics Window", string.Empty);
            _buildDiagnosticDocumentViewModel = _outputLogger.GetLogFromOutput("Build Diagnostics Window", string.Empty);
            InitItemSources();
            _directoryFactory = new TestDirectoryFactory(_source);
            _mutantInitializer = new MutantInitializer(_source);
            _previousMutants = new Dictionary<string, MutantStatus>();
            _isRealMutationAnalysisRunning = false;
            _isClosed = false;
        }

        private void InitItemSources()
        {
            var mutators = MutantOrchestrator.AllMutators;
            SelectedMutators.Clear();
            SelectedMutators.AddRange(mutators.Where(x => x.DefaultMutant).ToList());

            MutantList.ItemsSource = mutators;
        }

        public static MutantViewerViewModel Create(SourceClassDetail source)
        {
            return ViewModelSource.Create(() => new MutantViewerViewModel(source));
        }

        public async Task BtnFindMutantsClick(object selectedItems)
        {
            try
            {
                _idle = false;
                _testsAreFailing = false;
                _selectedMutators = selectedItems;
                IsSplashScreenShown = true;
                DisableMutators = true;
                var selectedMutators = (ObservableCollection<object>)selectedItems;
                if (!selectedMutators.Any())
                {
                    MessageBoxService.Show(SelectAtLeastOneMutator);
                    return;
                }

                var buildPassed = true;
                if (ChkOptimizeTestProject.IsChecked &&
                    !_source.TestClaz.DoNetCoreProject)
                {
                    var originalProject = _source.TestClaz.ClassProject;
                    _source.TestClaz.ClassProject = originalProject.UpdateTestProject(_source.TestClaz.Claz.ClassName());

                    buildPassed = await ExecuteBuild();

                    if (!originalProject.Equals(_source.TestClaz.ClassProject, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (File.Exists(_source.TestClaz.ClassProject))
                        {
                            File.Delete(_source.TestClaz.ClassProject);
                        }

                        _source.TestClaz.ClassProject = originalProject;
                    }
                }

                if (buildPassed)
                {
                    if (!await ExecuteTests())
                    {
                        if (!_silently)
                        {
                            MessageBoxService.Show("Tests are failing! Please Fix Tests");
                        }

                        _testsAreFailing = true;
                        _idle = true;
                        return;
                    }

                    await InitializeMutants(selectedMutators);

                    PrintMutants();
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Mutants Click Found {0}", exp);
                MessageBoxService.Show(exp.Message);
            }
            finally
            {
                IsSplashScreenShown = false;
            }
        }

        private async Task<bool> ExecuteBuild()
        {
            try
            {
                var buildLog = _outputLogger.GetLogFromOutput(MSBuildOutput, string.Empty);
                var document = MutationDocumentManagerService.CreateDocument(nameof(CommandPromptOutputViewer), buildLog);
                document.Title = MSBuildOutput;
                document.Show();

                buildLog.CommandPromptOutput += "Building Test Project...".PrintWithPreTag();
                void OutputData(object sender, string args) => buildLog.CommandPromptOutput += args.Encode().PrintWithPreTag();
                var testCodeBuild = new BuildExecutor(Settings, _source.TestClaz.ClassProject)
                {
                    BaseAddress = Settings.ServiceAddress,
                    EnableLogging = ChkEnableDiagnostic.IsChecked,
                    QuietWithSymbols = true
                };
                testCodeBuild.OutputDataReceived += OutputData;
                if (_source.BuildInReleaseMode)
                {
                    await testCodeBuild.ExecuteBuildInReleaseModeWithoutDependencies();
                }
                else
                {
                    await testCodeBuild.ExecuteBuildInDebugModeWithoutDependencies();
                }

                testCodeBuild.OutputDataReceived -= OutputData;

                if (testCodeBuild.LastBuildStatus != BuildExecutionStatus.Failed)
                {
                    document.Close();
                }
                else
                {
                    MessageBoxService.Show("\nTest Project Build is failed!");
                }

                return testCodeBuild.LastBuildStatus == BuildExecutionStatus.Success;
            }
            catch (Exception e)
            {
                MessageBoxService.Show(e.Message);
                Trace.TraceError("Unknown Exception Occurred On Test Project Build {0}", e);
                return false;
            }
        }

        public async Task<bool> ExecuteTests()
        {
            try
            {
                var log = _outputLogger.GetLogFromOutput(TestsExecutionOutput, string.Empty);
                var document = MutationDocumentManagerService.CreateDocument(nameof(CommandPromptOutputViewer), log);
                var test = new TestExecutor(Settings, _source.TestClaz.ClassLibrary);
                document.Title = TestsExecutionOutput;
                if (!_silently)
                {
                    document.Show();
                }

                void OutputData(object sender, string args) => log.CommandPromptOutput += args.Encode().PrintWithPreTag();

                test.EnableCustomOptions = ChkEnableCodeCoverage.IsChecked;
                test.EnableLogging = false;
                test.OutputDataReceived += OutputData;
                test.X64TargetPlatform = _source.X64TargetPlatform;
                test.FullyQualifiedName = _source.TestClaz.MethodDetails.Count > Convert.ToInt32(Settings.UseClassFilterTestsThreshold) ||
                                          ChkUseClassFilter.IsChecked || _source.TestClaz.BaseClass != null
                    ? _source.TestClaz.Claz.FullName()
                    : string.Empty;

                if (_silently)
                {
                    test.FullyQualifiedName = _source.TestClaz.Claz.FullName();
                }

                await test.ExecuteTests(_source.TestClaz.MethodDetails);

                var coverageAnalyzer = new CoverageAnalyzer();
                coverageAnalyzer.FindCoverage(_source, test.CodeCoverage);

                if (test.LastTestExecutionStatus == TestExecutionStatus.Success)
                {
                    if (!_silently)
                    {
                        document.Close();
                    }

                    return true;
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Tests Execution Failed [{0}]", exp);
                MessageBoxService.Show(exp.Message);
            }

            return false;
        }

        private async Task InitializeMutants(IList<object> selectedMutators)
        {
            _mutantInitializer.ExecuteAllTests = ChkExecuteAllTests.IsChecked ||
                                                 _source.TestClaz.MethodDetails.Count > Convert.ToInt32(Settings.UseClassFilterTestsThreshold) || _silently;

            _mutantInitializer.MutantFilterId = MutantFilterId;
            _mutantInitializer.MutantFilterRegEx = MutantFilterRegEx;
            _mutantInitializer.SpecificFilterRegEx = SpecificMutantRegEx;
            _mutantInitializer.MutantsPerLine = Convert.ToInt32(MutantsPerLine);

            await _mutantInitializer.InitializeMutants(selectedMutators.Cast<IMutator>().ToList());

            if (_previousMutants.Any())
            {
                foreach (var mutant in _source.MethodDetails.SelectMany(x => x.Mutants))
                {
                    var mutationString = $"{mutant.Mutation.Location} - {mutant.Mutation.DisplayName}";
                    if (_previousMutants.ContainsKey(mutationString))
                    {
                        if (_previousMutants[mutationString] != MutantStatus.Skipped &&
                            _previousMutants[mutationString] != MutantStatus.NotCovered)
                        {
                            mutant.ResultStatus = _previousMutants[mutationString];
                        }
                    }
                }
            }
        }

        public void BtnCancelApplyingMutationClick()
        {
            if (_mutantExecutor != null)
            {
                _mutantExecutor.CancelMutationOperation = true;
            }
        }

        public async Task BtnApplyMutantsClick()
        {
            _numberOfMutantsExecutingInParallel = Convert.ToByte(NumberOfMutantsExecutedInParallel);
            if (SelectedMutants.All(x => x.Level != MutantLevel.Mutant))
            {
                if (!_silently)
                {
                    MessageBoxService.Show("Click Find Mutants Or Select At Least One Mutant");
                }

                return;
            }

            try
            {
                await InitializeMutants((ObservableCollection<object>)_selectedMutators);
                IsSplashScreenShown = true;
                if (ChkEnableDiagnostic.IsChecked)
                {
                    _testDiagnosticDocument = MutationDocumentManagerService.CreateDocument(
                        nameof(CommandPromptOutputViewer), _testDiagnosticDocumentViewModel);
                    _buildDiagnosticDocument = MutationDocumentManagerService.CreateDocument(
                        nameof(CommandPromptOutputViewer), _buildDiagnosticDocumentViewModel);

                    _testDiagnosticDocument.Show();
                    _buildDiagnosticDocument.Show();
                }

                MutantOperationsEnabled = false;
                ProgressBarMutationVisible = Visibility.Visible;
                MinimumProgress = 0;
                CurrentProgress = 0;

                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _mutationProcessLog = new StringBuilder();

                if (!_source.MethodDetails.SelectMany(x => x.Mutants).Any())
                {
                    if (!_silently)
                    {
                        MessageBoxService.Show(MutantsNotExist);
                    }

                    return;
                }

                var selectedMutants = SelectedMutants.Where(x => x.Level == MutantLevel.Mutant).ToList();
                foreach (var mutant in _source.MethodDetails.SelectMany(x => x.Mutants))
                {
                    if (selectedMutants.All(x => x.MutantId != mutant.Id && mutant.ResultStatus == MutantStatus.NotRun))
                    {
                        mutant.ResultStatus = MutantStatus.Skipped;
                    }
                }

                _directoryFactory.BuildExtensions = BuildExtensions;
                _directoryFactory.NumberOfMutantsExecutingInParallel = _numberOfMutantsExecutingInParallel;
                _directoryFactory.DeleteDirectories();
                await _directoryFactory.PrepareDirectoriesAndFiles();
                var methodDetails = _source.MethodDetails.Where(x => x.TestMethods.Any()).ToList();
                MaximumProgress = methodDetails.SelectMany(x => x.NotRunMutants).Count();

                if (!methodDetails.Any())
                {
                    if (!_silently)
                    {
                        MessageBoxService.Show(NoAnyTestsExist);
                    }

                    return;
                }

                IsSplashScreenShown = false;
                _mutantExecutor = new MutantExecutor(_source, Settings)
                {
                    EnableDiagnostics = ChkEnableDiagnostic.IsChecked,
                    NumberOfMutantsExecutingInParallel = _numberOfMutantsExecutingInParallel,
                    UseClassFilter = ChkUseClassFilter.IsChecked ||
                                     _source.TestClaz.MethodDetails.Count > Convert.ToInt32(Settings.UseClassFilterTestsThreshold) ||
                                     _source.TestClaz.BaseClass != null,
                    BaseAddress = Settings.ServiceAddress
                };

                if (_silently)
                {
                    _mutantExecutor.UseClassFilter = true;
                }

                _testDiagnosticDocumentViewModel.CommandPromptOutput = HtmlTemplate;
                _buildDiagnosticDocumentViewModel.CommandPromptOutput = HtmlTemplate;

                void MutantExecutorOnMutantExecuted(object sender, MutantEventArgs e)
                {
                    if (ChkEnableDiagnostic.IsChecked)
                    {
                        _testDiagnosticDocumentViewModel.CommandPromptOutput += e.TestLog;
                        _buildDiagnosticDocumentViewModel.CommandPromptOutput += e.BuildLog;
                    }

                    CurrentProgress++;
                }

                _mutantExecutor.MutantExecuted += MutantExecutorOnMutantExecuted;
                await _mutantExecutor.ExecuteMutants();
                _mutationProcessLog.Append(_mutantExecutor.LastExecutionOutput);
                _mutantExecutor.MutantExecuted -= MutantExecutorOnMutantExecuted;

                if (!_silently &&
                    ChkAnalyzeExternalCoverage.IsChecked &&
                    _source.ExternalCoveredClassesIncluded.Any())
                {
                    const string title = "Analyzing External Coverage";
                    var externalCoverageLog = _outputLogger.GetLogFromOutput(title, string.Empty);
                    externalCoverageLog.CommandPromptOutput = HtmlTemplate;
                    var chalkHtml = new ChalkHtml();
                    void OutputDataReceived(object sender, string output) => externalCoverageLog.CommandPromptOutput += output;
                    chalkHtml.OutputDataReceived += OutputDataReceived;

                    var mutantAnalyzer = new MutantAnalyzer(chalkHtml, Settings)
                    {
                        BuildInReleaseMode = _source.BuildInReleaseMode,
                        ConcurrentTestRunners = _numberOfMutantsExecutingInParallel,
                        EnableDiagnostics = ChkEnableDiagnostic.IsChecked,
                        ExecuteAllTests = ChkExecuteAllTests.IsChecked,
                        IncludeNestedClasses = _source.IncludeNestedClasses,
                        IncludePartialClasses = _source.TestClaz.PartialClassNodesAdded,
                        SourceProjectLibrary = _source.ClassLibrary,
                        SurvivedThreshold = 0.01,
                        TestClass = _source.TestClaz.FilePath,
                        TestProject = _source.TestClaz.ClassProject,
                        TestProjectLibrary = _source.TestClaz.ClassLibrary,
                        UseClassFilter = ChkUseClassFilter.IsChecked,
                        X64TargetPlatform = _source.X64TargetPlatform,
                        UseExternalCodeCoverage = true,
                        ProgressIndicator = '*'
                    };

                    var document = MutationDocumentManagerService.CreateDocument(nameof(CommandPromptOutputViewer), externalCoverageLog);
                    document.Title = title;
                    document.Show();
                    foreach (var acc in _source.ExternalCoveredClassesIncluded)
                    {
                        mutantAnalyzer.ExternalCoveredMutants.AddRange(acc.MutantsLines);
                        var projectFile = new FileInfo(acc.ClassPath).FindProjectFile();
                        mutantAnalyzer.SourceProjectLibrary = projectFile.FindLibraryPath()?.FullName;
                        if (!string.IsNullOrWhiteSpace(mutantAnalyzer.SourceProjectLibrary))
                        {
                            var accClass = await mutantAnalyzer.Analyze(
                                acc.ClassPath,
                                acc.ClassName,
                                projectFile.FullName);

                            accClass.CalculateMutationScore();

                            if (accClass.MutationScore.Survived == 0)
                            {
                                acc.ZeroSurvivedMutants = true;
                            }
                        }
                    }

                    chalkHtml.OutputDataReceived -= OutputDataReceived;
                    mutantAnalyzer.ExternalCoveredMutants.Clear();
                }

                stopWatch.Stop();
                _mutantExecutor.PrintMutatorSummary(_mutationProcessLog);
                _mutantExecutor.PrintClassSummary(_mutationProcessLog);
                _mutationProcessLog.AppendLine("<fieldset style=\"margin-bottom:10; margin-top:10;\">");
                _mutationProcessLog.AppendLine("Execution Time: ".PrintImportantWithLegend());
                _mutationProcessLog.Append($"{stopWatch.Elapsed}".PrintWithPreTagWithMarginImportant());
                _mutationProcessLog.AppendLine("</fieldset>");

                _previousMutants.Clear();
                foreach (var mutant in _source.MethodDetails.SelectMany(x => x.Mutants))
                {
                    var key = $"{mutant.Mutation.Location} - {mutant.Mutation.DisplayName}";
                    if (!_previousMutants.ContainsKey(key))
                    {
                        _previousMutants.Add(key, mutant.ResultStatus);
                    }
                }

                PrintMutants();
                RunFileWatcherService();
            }
            catch (Exception exception)
            {
                Trace.TraceError("Unknown Exception Occurred by Mutation Analyzer {0}", exception.StackTrace);
                MessageBoxService.Show(exception.Message);
            }
            finally
            {
                MutantOperationsEnabled = true;
                IsSplashScreenShown = false;
                ProgressBarMutationVisible = Visibility.Hidden;
                _directoryFactory.DeleteDirectories();
                _idle = true;
                _silently = false;
                var notification = NotificationService.CreatePredefinedNotification(MutationIsCompletedNotification, string.Empty, string.Empty);
                await notification.ShowAsync();
            }
        }

        public void PrintReport()
        {
            if (_mutationProcessLog == null || string.IsNullOrWhiteSpace(_mutationProcessLog.ToString()))
            {
                MessageBoxService.Show("No any Report Exist");
                return;
            }

            _printDocument = MutationDocumentManagerService.CreateDocument(
                nameof(CommandPromptOutputViewer),
                _outputLogger.GetLogFromOutput($"{_source.Claz.ClassName()}_Dynamic_Mutation_Analysis", _mutationProcessLog.ToString()));

            _printDocument.Show();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void RunFileWatcherService()
        {
            if (!_isRealMutationAnalysisRunning)
            {
                try
                {
                    _watcher = new FileSystemWatcher
                    {
                        Path = Path.GetDirectoryName(_source.TestClaz.ClassLibrary),
                        NotifyFilter = NotifyFilters.LastWrite,
                        Filter = Path.GetFileName(_source.TestClaz.ClassLibrary)
                    };

                    _watcher.Changed += OnChanged;
                    _watcher.EnableRaisingEvents = true;
                }
                catch (Exception e)
                {
                    Trace.TraceError("Closing Real Time Mutation {0}", e);
                    _watcher?.Dispose();
                }
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!_isClosed)
            {
                if (ChkRealTimeAnalysis.IsChecked)
                {
                    try
                    {
                        Application.Current.Dispatcher?.BeginInvoke(new Action(async () =>
                        {
                            if (_idle)
                            {
                                _silently = true;
                                await BtnFindMutantsClick(_selectedMutators);
                                if (!_testsAreFailing)
                                {
                                    await BtnApplyMutantsClick();
                                }
                            }
                        }), DispatcherPriority.ApplicationIdle);
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceError("Closing Real Time Mutation {0}", exception);
                        _watcher?.Dispose();
                    }
                }
            }
            else
            {
                _watcher?.Dispose();
            }
        }

        private void PrintMutants()
        {
            var mutants = _source.MethodDetails.SelectMany(x => x.Mutants).ToList();
            var mutantCount = mutants.Count;
            var skipped = mutants.Count(x => x.ResultStatus == MutantStatus.Skipped);
            var uncovered = mutants.Count(x => x.ResultStatus == MutantStatus.NotCovered);
            const int classId = 1;
            var mutantClass = MutantsDetails.FirstOrDefault(x => x.Id == classId);
            var fontSize = 3;
            if (mutantClass != null)
            {
                mutantClass.Name = $"Class: {_source.FullName} - Mutants: (Total: {mutantCount}) (Uncovered: {uncovered}) (Skipped: {skipped})".PrintImportant(fontSize);
            }
            else
            {
                MutantsDetails.Add(new MutantDetail
                {
                    Name = $"Class: {_source.FullName} - Mutants: (Total: {mutantCount}) (Uncovered: {uncovered}) (Skipped: {skipped})".PrintImportant(fontSize),
                    Id = classId,
                    Level = MutantLevel.Class
                });
            }

            if (mutantCount > 0)
            {
                var methodId = classId + 1;
                var mutantId = _source.MethodDetails.Count + methodId;
                foreach (var method in _source.MethodDetails)
                {
                    methodId++;
                    if (method.Mutants.Any())
                    {
                        var mutantMethod = MutantsDetails.FirstOrDefault(x => x.Id == methodId);
                        var methodBody =
                            $"Method: {method.MethodName} - Mutants: (Total: {method.Mutants.Count}) (Uncovered: {method.NotCoveredMutants.Count}) (Skipped: {method.SkippedMutants.Count})"
                                .PrintImportant(fontSize);
                        if (mutantMethod != null)
                        {
                            mutantMethod.Name = methodBody;
                        }
                        else
                        {
                            MutantsDetails.Add(new MutantDetail
                            {
                                Id = methodId,
                                Name = methodBody,
                                ParentId = classId,
                                Level = MutantLevel.Method
                            });
                        }

                        foreach (var mutant in method.Mutants)
                        {
                            var mutantMutation = mutant.Mutation;
                            mutantId++;
                            var mutantFontSize = 2.5;
                            var status = mutant.ResultStatus == MutantStatus.Killed
                                ? "Killed".PrintImportant(fontSize, Colors.Green)
                                : mutant.ResultStatus == MutantStatus.Survived
                                    ? "Survived".PrintImportant(fontSize, Colors.Red)
                                    : mutant.ResultStatus == MutantStatus.Skipped
                                        ? "Skipped".PrintImportant(fontSize, Colors.Orange)
                                        : mutant.ResultStatus.ToString().PrintImportant(fontSize);
                            if (mutantMutation.ReplacementNode != null)
                            {
                                var originalNode = mutantMutation.OriginalNode.ToString().Encode().Print(mutantFontSize, Colors.Green);
                                var replacementNode = mutantMutation.ReplacementNode.ToString().Encode().Print(mutantFontSize, Colors.Red);
                                var mutantBody =
                                    $"{mutant.Id.ToString()} - Line: {mutant.Mutation.Location} - {mutantMutation.Type} - {originalNode} replace with {replacementNode} - {status}".Print(
                                        mutantFontSize);

                                var mutantDetail = MutantsDetails.FirstOrDefault(x => x.Id == mutantId);
                                if (mutantDetail != null)
                                {
                                    mutantDetail.Name = mutantBody;
                                }
                                else
                                {
                                    MutantsDetails.Add(new MutantDetail
                                    {
                                        Id = mutantId,
                                        Name = mutantBody,
                                        ParentId = methodId,
                                        Level = MutantLevel.Mutant,
                                        MutantId = mutant.Id
                                    });
                                }
                            }
                            else
                            {
                                var originalNode = mutantMutation.OriginalNode.ToString().Print(mutantFontSize, Colors.Red);
                                var mutantBody = $"{mutantMutation.Type} - remove {originalNode} - {status}".Print(mutantFontSize);
                                var mutantDetail = MutantsDetails.FirstOrDefault(x => x.Id == mutantId);
                                if (mutantDetail != null)
                                {
                                    mutantDetail.Name = mutantBody;
                                }
                                else
                                {
                                    MutantsDetails.Add(new MutantDetail
                                    {
                                        Id = mutantId,
                                        Name = mutantBody,
                                        ParentId = methodId,
                                        Level = MutantLevel.Mutant,
                                        MutantId = mutant.Id
                                    });
                                }
                            }
                        }
                    }
                }
            }

            var dummy = new MutantDetail
            {
                Id = -1,
                ParentId = -1
            };
            MutantsDetails.Add(dummy);
            MutantsDetails.Remove(dummy);
        }

        public void Unload()
        {
            _watcher?.Dispose();
            BtnCancelApplyingMutationClick();
            _isClosed = true;
        }
    }
}