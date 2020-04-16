using System;
using System.Diagnostics;
using System.IO;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Microsoft.WindowsAPICodePack.Dialogs;
using static MuTest.Core.Common.Constants;

namespace Dashboard.ViewModel
{
    [POCOViewModel]
    public class CommandPromptOutputViewerViewModel
    {
        public virtual string CommandPromptOutput { get; set; }

        public virtual string Header { get; }

        [ServiceProperty(Key = "MessageBoxService")]
        protected virtual IMessageBoxService MessageBoxService => null;

        public CommandPromptOutputViewerViewModel(string header)
        {
            Header = header;
        }

        protected CommandPromptOutputViewerViewModel()
        {
        }

        public static CommandPromptOutputViewerViewModel Create(string header)
        {
            return ViewModelSource.Create(() => new CommandPromptOutputViewerViewModel(header));
        }

        public void BtnExportToHtmlClick()
        {
            if (string.IsNullOrWhiteSpace(CommandPromptOutput))
            {
                MessageBoxService.Show(ExportToHtmlErrorMessage);
                return;
            }

            try
            {
                using (var dialog = new CommonSaveFileDialog
                {
                    DefaultFileName = Header,
                    DefaultExtension = HtmlFile,
                    DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory, Environment.SpecialFolderOption.None)
                })
                {
                    var result = dialog.ShowDialog();

                    if (result == CommonFileDialogResult.Ok)
                    {
                        File.WriteAllText(dialog.FileName, CommandPromptOutput);
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unknown Exception Occurred On Exporting data to html {0}", exp);
                MessageBoxService.Show(ErrorMessage);
            }
        }
    }
}