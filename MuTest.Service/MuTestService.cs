using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using Microsoft.Owin.Hosting;
using MuTest.Core.Common.Settings;

namespace MuTest.Service
{
    public partial class MuTestService : ServiceBase
    {
        private static readonly MuTestSettings Settings = MuTestSettingsSection.GetSettings();
        public static string BaseAddress { get; private set; }
        private IDisposable _server;

        public MuTestService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Trace.Listeners.Add(new EventLogTraceListener("MuTestService"));
            if (args.Length == 0 || !string.IsNullOrWhiteSpace(Settings.ServiceAddress))
            {
                args = new[]
                {
                    Settings.ServiceAddress.TrimEnd('/')
                };
            }

            if (args.Length == 0 ||
                !Regex.IsMatch(args[0], @"^(http|https):\/\/((([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])|localhost):[0-9]+$", RegexOptions.IgnoreCase))
            {
                var errorMessage = "Please add valid service host address as start parameter: xxx.xxx.xxx.xxx:port";
                throw new InvalidOperationException(errorMessage);
            }

            BaseAddress = args[0];
            _server = WebApp.Start<Startup>(BaseAddress);
        }

        protected override void OnStop()
        {
            _server?.Dispose();
            base.OnStop();
        }
    }
}