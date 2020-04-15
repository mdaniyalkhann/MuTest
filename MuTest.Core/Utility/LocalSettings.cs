using System;
using System.Collections.Generic;
using System.IO;

namespace MuTest.Core.Utility
{
    public static class LocalSettings
    {
        private const string SettingsFile = "settings.txt";

        public static string Get(string key)
        {
            if (GetAll().TryGetValue(key, out var settingValue))
            {
                return settingValue;
            }

            return null;
        }

        public static void Set(string key, string item)
        {
            var settingsDictionary = GetAll();

            if (settingsDictionary.ContainsKey(key))
            {
                settingsDictionary.Remove(key);
            }

            settingsDictionary.Add(key, item);

            SetAll(settingsDictionary);
        }

        private static void SetAll(IDictionary<string, string> settings)
        {
            var settingLines = new List<string>();

            foreach (var key in settings.Keys)
            {
                settingLines.Add($"{key}={settings[key]}");
            }

            File.WriteAllLines(GetSettingsFileInfo().FullName, settingLines.ToArray());
        }

        public static IDictionary<string, string> GetAll()
        {
            var settingsFileInfo = GetSettingsFileInfo();

            var settingsDictionary = new Dictionary<string, string>();

            if (!settingsFileInfo.Exists)
            {
                return settingsDictionary;
            }

            var settingLines = File.ReadAllLines(settingsFileInfo.FullName);

            foreach (var settingLine in settingLines)
            {
                var settingParts = settingLine.Split('=');

                if (settingParts.Length == 2)
                {
                    settingsDictionary.Add(settingParts[0], settingParts[1]);
                }
            }

            return settingsDictionary;
        }

        private static FileInfo GetSettingsFileInfo()
        {
            var appDataFolder = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData, 
                Environment.SpecialFolderOption.Create);

            var settingsFileName = Path.Combine(appDataFolder, SettingsFile);
            var settingsFileInfo = new FileInfo(settingsFileName);
            return settingsFileInfo;
        }
    }
}
