using Dashboard.ViewModel;

namespace Dashboard.Common
{
    public interface ICommandPromptOutputLogger
    {
        CommandPromptOutputViewerViewModel GetLog(string header, string logFile);

        CommandPromptOutputViewerViewModel GetLogFromOutput(string header, string output);
    }
}