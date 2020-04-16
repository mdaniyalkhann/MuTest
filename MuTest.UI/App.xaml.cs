using System.Diagnostics;
using System.Windows;
using DevExpress.Xpf.Core;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {
            ApplicationThemeHelper.UpdateApplicationThemeName();
            DevExpress.Data.ShellHelper.TryCreateShortcut("mutest-notification", "NotificationService");
            Trace.Listeners.Add(new EventLogTraceListener("MuTestUI"));
        }
    }
}
