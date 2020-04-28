using System;
using System.Configuration;

namespace MuTest.Core.Common.Settings
{
    public class MuTestSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty(nameof(MuTestSettings))]
        public MuTestSettings MuTestSettings
        {
            get => (MuTestSettings)this[nameof(MuTestSettings)];
            set => value = (MuTestSettings)this[nameof(MuTestSettings)];
        }

        public static MuTestSettings GetSettings()
        {
            var section = ConfigurationManager.GetSection(nameof(MuTestSettingsSection)) as MuTestSettingsSection;
            if (section == null)
            {
                throw new InvalidOperationException($"Section {nameof(MuTestSettingsSection)} not found");
            }

            return section.MuTestSettings;
        }
    }
}