using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Dashboard.ViewModel;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Editors;
using MuTest.Core.Utility;

namespace Dashboard.Views
{
    /// <summary>
    /// Interaction logic for ShimGenerator.xaml
    /// </summary>
    public partial class Dashboard : DXWindow
    {
        public Dashboard()
        {
            InitializeComponent();
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyVersion = assembly.GetName().Version;
            Title = $"C# MuTest Tool {assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
        }

        private const string ClassLookupDisplayText = "Select Class...";
        private const string TestClassLookupDisplayText = "Select Test Class...";
        private const string TextFormat = "Text";
        private const string CSharpFileExtension = ".cs";

        private void ClassLookup_OnCustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            SetDisplayText(e, ClassLookupDisplayText);
        }

        private void TestClassLookup_OnCustomDisplayText(object sender, CustomDisplayTextEventArgs e)
        {
            SetDisplayText(e, TestClassLookupDisplayText);
        }

        private void BtnEdit_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        private static void SetDisplayText(CustomDisplayTextEventArgs e, string displayText)
        {
            if (string.IsNullOrWhiteSpace(e.DisplayText))
            {
                e.DisplayText = displayText;
                e.Handled = true;
            }
        }

        private void BtnSourceEdit_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var vm = (DashboardViewModel)DataContext;
            try
            {
                LoadingDecoratorMain.IsSplashScreenShown = true;
                var data = e.Data.GetData(TextFormat);
                if (data != null &&
                    data is string fileName &&
                    fileName.EndsWith(CSharpFileExtension))
                {
                    var file = new FileInfo(fileName);
                    if (file.Exists)
                    {
                        var projectFile = file.FindProjectFile();
                        if (projectFile != null)
                        {
                            vm.InitSourceCode(projectFile.DirectoryName, fileName);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
                vm.ShowMessage("Unable to Load Source Class");
            }
            finally
            {
                LoadingDecoratorMain.IsSplashScreenShown = false;
            }
        }

        private void BtnTestEdit_OnPreviewDrop(object sender, DragEventArgs e)
        {
            var vm = (DashboardViewModel)DataContext;
            try
            {
                LoadingDecoratorMain.IsSplashScreenShown = true;
                var data = e.Data.GetData(TextFormat);
                if (data != null &&
                    data is string fileName &&
                    fileName.EndsWith(CSharpFileExtension))
                {
                    var file = new FileInfo(fileName);
                    if (file.Exists)
                    {
                        var projectFile = file.FindProjectFile();
                        if (projectFile != null)
                        {
                            vm.InitTestSource(projectFile.DirectoryName, fileName);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception);
                vm.ShowMessage("Unable to Load Test Class");
            }
            finally
            {
                LoadingDecoratorMain.IsSplashScreenShown = false;
            }
        }
    }
}