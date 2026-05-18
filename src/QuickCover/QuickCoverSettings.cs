using System.Collections.Generic;
using System.IO;
using Playnite.SDK;

namespace QuickCover
{
    public class QuickCoverSettings : ISettings
    {
        private readonly QuickCoverPlugin plugin;
        private string editingDefaultCoverImagePath;
        private string editingDefaultBackgroundImagePath;

        public string DefaultCoverImagePath { get; set; } = string.Empty;

        public string DefaultBackgroundImagePath { get; set; } = string.Empty;

        public QuickCoverSettings()
        {
        }

        public QuickCoverSettings(QuickCoverPlugin plugin)
        {
            this.plugin = plugin;

            var savedSettings = plugin.LoadPluginSettings<QuickCoverSettings>();
            if (savedSettings != null)
            {
                DefaultCoverImagePath = savedSettings.DefaultCoverImagePath ?? string.Empty;
                DefaultBackgroundImagePath = savedSettings.DefaultBackgroundImagePath ?? string.Empty;
            }
        }

        public void BeginEdit()
        {
            editingDefaultCoverImagePath = DefaultCoverImagePath;
            editingDefaultBackgroundImagePath = DefaultBackgroundImagePath;
        }

        public void CancelEdit()
        {
            DefaultCoverImagePath = editingDefaultCoverImagePath;
            DefaultBackgroundImagePath = editingDefaultBackgroundImagePath;
        }

        public void EndEdit()
        {
            plugin?.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            ValidateConfiguredPath(DefaultCoverImagePath, "Default cover image", errors);
            ValidateConfiguredPath(DefaultBackgroundImagePath, "Default background image", errors);

            return errors.Count == 0;
        }

        private static void ValidateConfiguredPath(string imagePath, string settingName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            if (!File.Exists(imagePath))
            {
                errors.Add($"{settingName} file was not found.");
            }
        }
    }
}
