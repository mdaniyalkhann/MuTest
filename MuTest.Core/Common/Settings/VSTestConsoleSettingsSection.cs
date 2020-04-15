using System;
using System.Configuration;

namespace MuTest.Core.Common.Settings
{
    public class VSTestConsoleSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty(nameof(VSTestConsoleSettings))]
        public VSTestConsoleSettings VSTestConsoleSettings
        {
            get => (VSTestConsoleSettings)this[nameof(VSTestConsoleSettings)];
            set => value = (VSTestConsoleSettings)this[nameof(VSTestConsoleSettings)];
        }

        public static VSTestConsoleSettings GetSettings()
        {
            var section = ConfigurationManager.GetSection(nameof(VSTestConsoleSettingsSection)) as VSTestConsoleSettingsSection;
            if (section == null)
            {
                throw new InvalidOperationException($"Section {nameof(VSTestConsoleSettingsSection)} not found");
            }

            return section.VSTestConsoleSettings;
        }
    }
}