using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Dashboard.Views;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.WindowsAPICodePack.Dialogs;
using MuTest.Core.Model;
using MuTest.Core.Model.ClassDeclarations;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class DashboardViewModel
    {
        public virtual string SelectSourceCodeDll { get; set; } = "Select Source Code DLL...";

        public virtual string SelectTestCodeDll { get; set; } = "Select Test Code DLL...";

        public virtual string SelectTestProject { get; set; } = "Select Test Project...";

        public virtual string SelectSourceCodeProject { get; set; } = "Select Source Code Project...";

        public virtual bool IsSplashScreenShown { get; set; }

        public virtual ControlViewModel BtnAnalyze { get; }

        public virtual LookupEditViewModel ClassLookup { get; }

        public virtual LookupEditViewModel TestClassLookup { get; }

        [ServiceProperty(SearchMode = ServiceSearchMode.PreferParents)]
        protected virtual IDocumentManagerService DocumentManagerService => null;

        public ControlViewModel ChkIncludePartialClasses { get; }

        public ControlViewModel ChkNestedClasses { get; }

        public virtual bool TopMost { get; set; } = true;

        [ServiceProperty(Key = "MessageBoxService")]
        protected virtual IMessageBoxService MessageBoxService => null;

        private static ConcurrentBag<SyntaxFile> _compilationUnitSyntaxList;
        private static ConcurrentBag<SyntaxFile> _testCompilationUnitSyntaxList;
        private SourceClassDetail _selectedClass;
        private TestClassDetail _selectedTestClass;
        private IList<TestClassDetail> _testLookUpDataSource;
        private string _selectedSourceClassPath;
        private string _selectedTestClassPath;

        protected DashboardViewModel()
        {
            ClassLookup = LookupEditViewModel.CreateLookupEdit();
            TestClassLookup = LookupEditViewModel.CreateLookupEdit();
            BtnAnalyze = ControlViewModel.Create();
            ChkIncludePartialClasses = ControlViewModel.CreateWithChecked();
            ChkNestedClasses = ControlViewModel.Create();

            InitializeFolders();
        }

        public void ShowMessage(string message)
        {
            MessageBoxService.Show(message);
        }

        public void GotFocus()
        {
            TopMost = !DocumentManagerService.Documents.Any();
        }

        public static DashboardViewModel Create()
        {
            return ViewModelSource.Create(() => new DashboardViewModel());
        }

        public void InitSourceCode(string directoryName, string selectedFileNamePath = null)
        {
            _selectedSourceClassPath = selectedFileNamePath;
            Reset(ClassLookup);
            StoreBrowseFolderSetting(SourceCodeDllSetting, directoryName);
            StoreBrowseFolderSetting(SourceCodeProjectSetting, directoryName);

            var projectName = Path.GetFileName(directoryName);
            var sourceProject = directoryName.FindFile($"{projectName}.csproj");
            if (sourceProject != null)
            {
                SelectSourceCodeProject = sourceProject.FullName;
                StoreBrowseFolderSetting(SourceCodeProjectSetting, SelectSourceCodeProject);
            }

            if (sourceProject == null)
            {
                sourceProject = directoryName.FindProjectFile();
                if (sourceProject != null)
                {
                    SelectSourceCodeProject = sourceProject.FullName;
                    StoreBrowseFolderSetting(SourceCodeProjectSetting, SelectSourceCodeProject);
                }
            }

            var sourceDll = sourceProject.FindLibraryPath();
            if (sourceDll != null)
            {
                SelectSourceCodeDll = sourceDll.FullName;
                StoreBrowseFolderSetting(SourceCodeDllSetting, SelectSourceCodeDll);
            }

            InitializeClassLookup();
        }

        public void InitTestSource(string directoryName, string selectedTestClassPath = null)
        {
            _selectedTestClassPath = selectedTestClassPath;
            Reset(TestClassLookup);
            StoreBrowseFolderSetting(TestCodeDllSetting, directoryName);
            StoreBrowseFolderSetting(TestCodeProjectSetting, directoryName);

            var projectName = Path.GetFileName(directoryName);
            var testProject = directoryName.FindFile($"{projectName}.csproj");
            if (testProject != null)
            {
                SelectTestProject = testProject.FullName;
                StoreBrowseFolderSetting(TestCodeProjectSetting, SelectTestProject);
            }

            if (testProject == null)
            {
                testProject = directoryName.FindProjectFile();
                if (testProject != null)
                {
                    SelectTestProject = testProject.FullName;
                    StoreBrowseFolderSetting(TestCodeProjectSetting, SelectTestProject);
                }
            }

            var testDll = testProject.FindLibraryPath();
            if (testDll != null)
            {
                SelectTestCodeDll = testDll.FullName;
                StoreBrowseFolderSetting(TestCodeDllSetting, SelectTestCodeDll);
            }

            InitializeTestClassLookup();
        }

        public void BrowseSourceCodeDll()
        {
            try
            {
                using (var dialog = new CommonOpenFileDialog
                {
                    Filters =
                    {
                        new CommonFileDialogFilter(LibraryFilesFilter, string.Join(",", LibraryFile, ExecutableFile))
                    },
                    AddToMostRecentlyUsedList = true
                })
                {
                    var sourceCodeDll = LocalSettings.Get(SourceCodeDllSetting);
                    if (sourceCodeDll != null)
                    {
                        dialog.InitialDirectory = Directory.Exists(sourceCodeDll)
                            ? sourceCodeDll
                            : Path.GetDirectoryName(sourceCodeDll);
                    }

                    var result = dialog.ShowDialog();
                    IsSplashScreenShown = true;

                    if (result == CommonFileDialogResult.Ok)
                    {
                        SelectSourceCodeDll = dialog.FileName;
                        StoreBrowseFolderSetting(SourceCodeDllSetting, SelectSourceCodeDll);
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Setting Source Dll {0}", exp);
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        public void BrowseTestCodeDll()
        {
            try
            {
                using (var dialog = new CommonOpenFileDialog
                {
                    DefaultExtension = LibraryFile,
                    Filters =
                    {
                        new CommonFileDialogFilter(LibraryFilesFilter, LibraryFile)
                    },
                    AddToMostRecentlyUsedList = true
                })
                {
                    var testCodeDllSetting = LocalSettings.Get(TestCodeDllSetting);
                    if (testCodeDllSetting != null)
                    {
                        dialog.InitialDirectory = Directory.Exists(testCodeDllSetting)
                            ? testCodeDllSetting
                            : Path.GetDirectoryName(testCodeDllSetting);
                    }

                    var result = dialog.ShowDialog();
                    IsSplashScreenShown = true;

                    if (result == CommonFileDialogResult.Ok)
                    {
                        SelectTestCodeDll = dialog.FileName;
                        StoreBrowseFolderSetting(TestCodeDllSetting, SelectTestCodeDll);
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Setting Test Code DLL {0}", exp);
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        public void BrowseTestProject()
        {
            try
            {
                using (var dialog = new CommonOpenFileDialog
                {
                    DefaultExtension = CSharpProjectFile,
                    Filters =
                    {
                        new CommonFileDialogFilter(ProjectFilesFilter, CSharpProjectFile)
                    },
                    AddToMostRecentlyUsedList = true
                })
                {
                    var testProjectSettings = LocalSettings.Get(TestCodeProjectSetting);
                    if (testProjectSettings != null)
                    {
                        dialog.InitialDirectory = Directory.Exists(testProjectSettings)
                            ? testProjectSettings
                            : Path.GetDirectoryName(testProjectSettings);
                    }

                    var result = dialog.ShowDialog();
                    IsSplashScreenShown = true;

                    if (result == CommonFileDialogResult.Ok)
                    {
                        SelectTestProject = dialog.FileName;
                        StoreBrowseFolderSetting(TestCodeProjectSetting, SelectTestProject);
                        InitTestSource(new FileInfo(SelectTestProject).DirectoryName);
                        InitializeTestClassLookup();
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Setting Test Project {0}", exp);
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        public void BrowseSourceProject()
        {
            try
            {
                using (var dialog = new CommonOpenFileDialog
                {
                    DefaultExtension = CSharpProjectFile,
                    Filters =
                    {
                        new CommonFileDialogFilter(ProjectFilesFilter, CSharpProjectFile)
                    },
                    AddToMostRecentlyUsedList = true
                })
                {
                    var sourceProjectSettings = LocalSettings.Get(SourceCodeProjectSetting);
                    if (sourceProjectSettings != null)
                    {
                        dialog.InitialDirectory = Directory.Exists(sourceProjectSettings)
                            ? sourceProjectSettings
                            : Path.GetDirectoryName(sourceProjectSettings);
                    }

                    var result = dialog.ShowDialog();
                    IsSplashScreenShown = true;

                    if (result == CommonFileDialogResult.Ok)
                    {
                        SelectSourceCodeProject = dialog.FileName;
                        StoreBrowseFolderSetting(SourceCodeProjectSetting, SelectSourceCodeProject);
                        InitSourceCode(new FileInfo(SelectSourceCodeProject).DirectoryName);
                        InitializeClassLookup();
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Setting Source Project Build {0}", exp.ToString().TrimToTraceLimit());
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        public void ClassLookupSelectedIndexChanged(object selectedItem)
        {
            try
            {
                IsSplashScreenShown = true;
                _selectedClass = (SourceClassDetail)selectedItem;
                ShowAnalyzeButton();
            }
            catch (Exception)
            {
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        public void TestClassLookupSelectedIndexChanged(object selectedItem)
        {
            try
            {
                IsSplashScreenShown = true;
                _selectedTestClass = (TestClassDetail)selectedItem;
                ShowAnalyzeButton();
            }
            catch (Exception)
            {
                IsSplashScreenShown = false;
                MessageBoxService.Show(ErrorMessage);
            }

            IsSplashScreenShown = false;
        }

        private void ShowAnalyzeButton()
        {
            BtnAnalyze.Visibility = ClassLookup.SelectedIndex != -1 && TestClassLookup.SelectedIndex != -1
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        public void BtnAnalyzeClick()
        {
            IsSplashScreenShown = false;
            _selectedTestClass.ClassLibrary = SelectTestCodeDll;
            _selectedTestClass.ClassProject = SelectTestProject;

            _selectedTestClass.PartialClasses.Clear();
            _selectedTestClass.PartialClasses.Add(new ClassDetail
            {
                Claz = _selectedTestClass.Claz,
                FilePath = _selectedTestClass.FilePath
            });

            var baseListSyntax = _selectedTestClass.Claz.Syntax.BaseList;
            if (baseListSyntax != null &&
                baseListSyntax.Types.Any())
            {
                foreach (var type in baseListSyntax.Types)
                {
                    var typeSyntax = type.Type;
                    var fileName = typeSyntax.ToString();
                    if (typeSyntax is GenericNameSyntax syntax)
                    {
                        fileName = syntax.Identifier.ValueText;
                    }

                    var bfile = Path.GetDirectoryName(SelectTestProject)
                        .FindFile($"{fileName}.cs");
                    if (bfile?
                        .GetCodeFileContent()
                        .RootNode() is CompilationUnitSyntax baseFile)
                    {
                        _selectedTestClass.BaseClass = baseFile;
                    }
                }
            }

            if (ChkIncludePartialClasses.IsChecked)
            {
                foreach (var data in _testLookUpDataSource)
                {
                    if (!_selectedTestClass.PartialClassNodesAdded &&
                        _selectedTestClass.FullName == data.FullName &&
                        data.FilePath != _selectedTestClass.FilePath)
                    {
                        _selectedTestClass.PartialClasses.Add(data);
                    }
                }

                _selectedTestClass.PartialClassNodesAdded = true;
            }

            foreach (var data in _testLookUpDataSource)
            {
                if (_selectedTestClass.FullName == data.FullName)
                {
                    var setupMethod = data.Claz.Syntax.NUnitSetupMethod();
                    var tearDownMethod = data.Claz.Syntax.NUnitTearDownMethod();

                    if (setupMethod != null && tearDownMethod != null)
                    {
                        _selectedTestClass.PartialClassWithSetupLogic = data;
                        break;
                    }
                }
            }

            if (_selectedTestClass.PartialClassWithSetupLogic == null &&
                _selectedTestClass.BaseClass != null)
            {
                var baseClass = _selectedTestClass.BaseClass.DescendantNodes<ClassDeclarationSyntax>()
                    .FirstOrDefault(x => x.NUnitSetupMethod() != null && x.NUnitTearDownMethod() != null);

                if (baseClass != null)
                {
                    _selectedTestClass.SetupInBaseClass = true;
                    _selectedTestClass.PartialClassWithSetupLogic = _selectedTestClass;
                }
            }

            _selectedClass.TestClaz = _selectedTestClass;
            _selectedClass.ClassProject = SelectSourceCodeProject;
            _selectedClass.ClassLibrary = SelectSourceCodeDll;
            _selectedClass.IncludeNestedClasses = ChkNestedClasses.IsChecked;
            _selectedClass.DoNetCoreProject = _selectedClass.ClassProject.DoNetCoreProject();
            _selectedClass.TestClaz.DoNetCoreProject = _selectedClass.TestClaz.ClassProject.DoNetCoreProject();

            var document = DocumentManagerService.CreateDocument(
                nameof(ClassViewer),
                ClassViewerViewModel.Create(_selectedClass));

            TopMost = false;
            document.Title = _selectedClass.FullName;
            document.Show();
        }

        private static void Reset(LookupEditViewModel lookup)
        {
            lookup.ItemsSource = null;
            lookup.Visibility = Visibility.Hidden;
            ResetSelectedIndex(lookup);
        }

        private static void ResetSelectedIndex(LookupEditViewModel lookup)
        {
            lookup.SelectedIndex = -1;
        }

        private void InitializeFolders()
        {
            IsSplashScreenShown = true;
            Reset(ClassLookup);
            Reset(TestClassLookup);
            Task.Run(() =>
            {
                try
                {
                    var allSettings = LocalSettings.GetAll();

                    if (allSettings.TryGetValue(SourceCodeProjectSetting, out var sourceCodeProjectSetting))
                    {
                        if (File.Exists(sourceCodeProjectSetting))
                        {
                            SelectSourceCodeProject = sourceCodeProjectSetting;
                            InitSourceCode(new FileInfo(SelectSourceCodeProject).DirectoryName);
                            InitializeClassLookup();
                        }
                    }

                    if (allSettings.TryGetValue(TestCodeProjectSetting, out var testProjectSetting))
                    {
                        if (File.Exists(testProjectSetting))
                        {
                            SelectTestProject = testProjectSetting;
                            InitTestSource(new FileInfo(SelectTestProject).DirectoryName);
                            InitializeTestClassLookup();
                        }
                    }

                    if (allSettings.TryGetValue(SourceCodeDllSetting, out var sourceCodeDllSetting))
                    {
                        if (File.Exists(sourceCodeDllSetting))
                        {
                            SelectSourceCodeDll = sourceCodeDllSetting;
                        }
                    }

                    if (allSettings.TryGetValue(TestCodeDllSetting, out var testCodeDllSetting))
                    {
                        if (File.Exists(testCodeDllSetting))
                        {
                            SelectTestCodeDll = testCodeDllSetting;
                        }
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceError("{0}", e.ToString().TrimToTraceLimit());
                    ShowMessage(e.Message);
                }
                finally
                {
                    IsSplashScreenShown = false;
                }
            });
        }

        private static void StoreBrowseFolderSetting(string name, string path)
        {
            LocalSettings.Set(name, path);
        }

        private void InitializeClassLookup()
        {
            _compilationUnitSyntaxList = SelectSourceCodeProject.GetCSharpClassDeclarationsFromProject();

            if (!_compilationUnitSyntaxList.Any())
            {
                ClassLookup.Visibility = Visibility.Hidden;
                MessageBoxService.Show(NoAnyClassFound);
            }
            else
            {
                var sourceClassDetails = (from cu in _compilationUnitSyntaxList
                                          from classDeclarationSyntax in cu.CompilationUnitSyntax.DescendantNodes<ClassDeclarationSyntax>()
                                          select new SourceClassDetail
                                          {
                                              Id = 0,
                                              FullName = $"{cu.CompilationUnitSyntax.NameSpace()}.{classDeclarationSyntax.Identifier.Text}",
                                              FilePath = cu.FileName,
                                              TotalNumberOfMethods = classDeclarationSyntax.DescendantNodes<MethodDeclarationSyntax>().Count,
                                              Claz = new ClassDeclaration(classDeclarationSyntax)
                                          })
                    .OrderByDescending(x => x.TotalNumberOfMethods)
                    .ToList();

                var id = 0;
                foreach (var classDetail in sourceClassDetails)
                {
                    classDetail.Id = id++;
                }

                ClassLookup.ItemsSource = new BindingSource
                {
                    DataSource = sourceClassDetails
                };

                ResetSelectedIndex(ClassLookup);

                var selected = sourceClassDetails.FirstOrDefault(x => x.FilePath.Equals(_selectedSourceClassPath));
                if (!string.IsNullOrWhiteSpace(_selectedSourceClassPath) && selected != null)
                {
                    ClassLookup.SelectedIndex = selected.Id;
                }
                ClassLookup.Visibility = Visibility.Visible;
            }
        }

        private void InitializeTestClassLookup()
        {
            _testCompilationUnitSyntaxList = SelectTestProject.GetCSharpClassDeclarationsFromProject();

            if (!_testCompilationUnitSyntaxList.Any())
            {
                TestClassLookup.Visibility = Visibility.Hidden;
                MessageBoxService.Show(NoAnyClassFound);
            }
            else
            {
                var id = 0;
                _testLookUpDataSource = _testCompilationUnitSyntaxList
                    .SelectMany(cu => cu.CompilationUnitSyntax.DescendantNodes<ClassDeclarationSyntax>(),
                        (cu, classDeclarationSyntax) => new TestClassDetail
                        {
                            Id = 0,
                            FullName = $"{cu.CompilationUnitSyntax.NameSpace()}.{classDeclarationSyntax.Identifier.Text}",
                            FilePath = cu.FileName,
                            TotalNumberOfMethods = classDeclarationSyntax.DescendantNodes<MethodDeclarationSyntax>().Count,
                            Claz = new ClassDeclaration(classDeclarationSyntax)
                        }).Where(x => x.TotalNumberOfMethods > 0)
                    .OrderByDescending(x => x.TotalNumberOfMethods)
                    .ToList();

                foreach (var testClassDetail in _testLookUpDataSource)
                {
                    testClassDetail.Id = id++;
                }

                TestClassLookup.ItemsSource = new BindingSource
                {
                    DataSource = _testLookUpDataSource
                };

                ResetSelectedIndex(TestClassLookup);

                var selected = _testLookUpDataSource.FirstOrDefault(x => x.FilePath.Equals(_selectedTestClassPath));
                if (!string.IsNullOrWhiteSpace(_selectedTestClassPath) && selected != null)
                {
                    TestClassLookup.SelectedIndex = selected.Id;
                }

                TestClassLookup.Visibility = Visibility.Visible;
            }
        }
    }
}