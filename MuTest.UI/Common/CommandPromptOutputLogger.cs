using System;
using System.Diagnostics;
using System.IO;
using Dashboard.ViewModel;
using DevExpress.Xpf.Core.Native;
using MuTest.Core.Common;
using MuTest.Core.Utility;
using static MuTest.Core.Common.Constants;

namespace Dashboard.Common
{
    public class CommandPromptOutputLogger : ICommandPromptOutputLogger
    {
        public CommandPromptOutputViewerViewModel GetLog(string header, string logFile)
        {
            try
            {
                using (var fileStream = File.Open(logFile.Replace(@"""", string.Empty), FileMode.Open))
                {
                    var vm = CommandPromptOutputViewerViewModel.Create(header);
                    vm.CommandPromptOutput = HtmlTemplate;
                    vm.CommandPromptOutput += fileStream
                        .ReadToString().Encode()
                        .PrintWithPreTag(color: DefaultColor);
                    return vm;
                }
            }
            catch (Exception exp)
            {
                Trace.TraceError("Unable to Create Log {0}", exp);
            }

            return null;
        }

        public CommandPromptOutputViewerViewModel GetLogFromOutput(string header, string output)
        {
            var vm = CommandPromptOutputViewerViewModel.Create(header);
            vm.CommandPromptOutput = HtmlTemplate;
            vm.CommandPromptOutput += output.PrintWithPreTag(color: DefaultColor);
            return vm;
        }
    }
}