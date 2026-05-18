using System;
using System.Collections.Generic;
using System.IO;
using Playnite.SDK;

namespace QuickCover
{
    public class QuickCoverSettings : ISettings
    {
        private readonly QuickCoverPlugin plugin;
        private string editingDefaultCoverImagePath;
        private string editingDefaultCoverImageUrl;
        private string editingDefaultBackgroundImagePath;
        private string editingDefaultBackgroundImageUrl;

        public string DefaultCoverImagePath { get; set; } = string.Empty;

        public string DefaultCoverImageUrl { get; set; } = string.Empty;

        public string DefaultBackgroundImagePath { get; set; } = string.Empty;

        public string DefaultBackgroundImageUrl { get; set; } = string.Empty;

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
                DefaultCoverImageUrl = savedSettings.DefaultCoverImageUrl ?? string.Empty;
                DefaultBackgroundImagePath = savedSettings.DefaultBackgroundImagePath ?? string.Empty;
                DefaultBackgroundImageUrl = savedSettings.DefaultBackgroundImageUrl ?? string.Empty;
            }
        }

        public void BeginEdit()
        {
            editingDefaultCoverImagePath = DefaultCoverImagePath;
            editingDefaultCoverImageUrl = DefaultCoverImageUrl;
            editingDefaultBackgroundImagePath = DefaultBackgroundImagePath;
            editingDefaultBackgroundImageUrl = DefaultBackgroundImageUrl;
        }

        public void CancelEdit()
        {
            DefaultCoverImagePath = editingDefaultCoverImagePath;
            DefaultCoverImageUrl = editingDefaultCoverImageUrl;
            DefaultBackgroundImagePath = editingDefaultBackgroundImagePath;
            DefaultBackgroundImageUrl = editingDefaultBackgroundImageUrl;
        }

        public void EndEdit()
        {
            plugin?.SavePluginSettings(this);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();

            ValidateConfiguredPath(DefaultCoverImagePath, "Default cover image file", errors);
            ValidateConfiguredUrl(DefaultCoverImageUrl, "Default cover image URL", errors);
            ValidateConfiguredPath(DefaultBackgroundImagePath, "Default background image file", errors);
            ValidateConfiguredUrl(DefaultBackgroundImageUrl, "Default background image URL", errors);

            return errors.Count == 0;
        }

        public static bool IsValidHttpUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return false;
            }

            if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }

        private static void ValidateConfiguredPath(string imagePath, string settingName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            if (!File.Exists(imagePath))
            {
                errors.Add($"{settingName} was not found.");
            }
        }

        private static void ValidateConfiguredUrl(string imageUrl, string settingName, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return;
            }

            if (!IsValidHttpUrl(imageUrl))
            {
                errors.Add($"{settingName} must be a valid http or https URL.");
            }
        }
    }
}
