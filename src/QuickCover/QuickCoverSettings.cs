using System;
using System.Collections.Generic;
using System.IO;
using Playnite.SDK;

namespace QuickCover
{
    public class QuickCoverSettings : ISettings
    {
        private const int MaxPathHistoryItems = 10;

        private readonly QuickCoverPlugin plugin;
        private string editingDefaultCoverImagePath;
        private string editingDefaultCoverImageUrl;
        private string editingDefaultBackgroundImagePath;
        private string editingDefaultBackgroundImageUrl;
        private List<string> editingDefaultCoverImagePathHistory = new List<string>();
        private List<string> editingDefaultBackgroundImagePathHistory = new List<string>();
        private bool editingPathHistoryInitialized;

        public string DefaultCoverImagePath { get; set; } = string.Empty;

        public string DefaultCoverImageUrl { get; set; } = string.Empty;

        public string DefaultBackgroundImagePath { get; set; } = string.Empty;

        public string DefaultBackgroundImageUrl { get; set; } = string.Empty;

        public List<string> DefaultCoverImagePathHistory { get; set; } = new List<string>();

        public List<string> DefaultBackgroundImagePathHistory { get; set; } = new List<string>();

        public bool PathHistoryInitialized { get; set; }

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
                var legacyCoverPath = savedSettings.PathHistoryInitialized
                    ? string.Empty
                    : DefaultCoverImagePath;
                var legacyBackgroundPath = savedSettings.PathHistoryInitialized
                    ? string.Empty
                    : DefaultBackgroundImagePath;
                DefaultCoverImagePathHistory = NormalizePathHistory(
                    savedSettings.DefaultCoverImagePathHistory,
                    legacyCoverPath);
                DefaultBackgroundImagePathHistory = NormalizePathHistory(
                    savedSettings.DefaultBackgroundImagePathHistory,
                    legacyBackgroundPath);
            }

            PathHistoryInitialized = true;
        }

        public void BeginEdit()
        {
            editingDefaultCoverImagePath = DefaultCoverImagePath;
            editingDefaultCoverImageUrl = DefaultCoverImageUrl;
            editingDefaultBackgroundImagePath = DefaultBackgroundImagePath;
            editingDefaultBackgroundImageUrl = DefaultBackgroundImageUrl;
            editingDefaultCoverImagePathHistory = new List<string>(DefaultCoverImagePathHistory ?? new List<string>());
            editingDefaultBackgroundImagePathHistory = new List<string>(DefaultBackgroundImagePathHistory ?? new List<string>());
            editingPathHistoryInitialized = PathHistoryInitialized;
        }

        public void CancelEdit()
        {
            DefaultCoverImagePath = editingDefaultCoverImagePath;
            DefaultCoverImageUrl = editingDefaultCoverImageUrl;
            DefaultBackgroundImagePath = editingDefaultBackgroundImagePath;
            DefaultBackgroundImageUrl = editingDefaultBackgroundImageUrl;
            DefaultCoverImagePathHistory = new List<string>(editingDefaultCoverImagePathHistory);
            DefaultBackgroundImagePathHistory = new List<string>(editingDefaultBackgroundImagePathHistory);
            PathHistoryInitialized = editingPathHistoryInitialized;
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

        public void UseDefaultCoverImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            DefaultCoverImagePathHistory = DefaultCoverImagePathHistory ?? new List<string>();
            DefaultCoverImagePath = AddPathToHistory(DefaultCoverImagePathHistory, imagePath);
        }

        public void UseDefaultBackgroundImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            DefaultBackgroundImagePathHistory = DefaultBackgroundImagePathHistory ?? new List<string>();
            DefaultBackgroundImagePath = AddPathToHistory(DefaultBackgroundImagePathHistory, imagePath);
        }

        public void RemoveDefaultCoverImagePathHistory(string imagePath)
        {
            RemovePathFromHistory(DefaultCoverImagePathHistory, imagePath);
        }

        public void RemoveDefaultBackgroundImagePathHistory(string imagePath)
        {
            RemovePathFromHistory(DefaultBackgroundImagePathHistory, imagePath);
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

        private static List<string> NormalizePathHistory(IEnumerable<string> history, string currentPath)
        {
            var normalizedHistory = new List<string>();
            AppendUniquePath(normalizedHistory, currentPath);

            if (history != null)
            {
                foreach (var imagePath in history)
                {
                    if (normalizedHistory.Count >= MaxPathHistoryItems)
                    {
                        break;
                    }

                    AppendUniquePath(normalizedHistory, imagePath);
                }
            }

            return normalizedHistory;
        }

        private static string AddPathToHistory(List<string> history, string imagePath)
        {
            var normalizedPath = imagePath?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedPath))
            {
                return string.Empty;
            }

            RemovePathFromHistory(history, normalizedPath);
            history.Insert(0, normalizedPath);

            while (history.Count > MaxPathHistoryItems)
            {
                history.RemoveAt(history.Count - 1);
            }

            return normalizedPath;
        }

        private static void AppendUniquePath(List<string> history, string imagePath)
        {
            var normalizedPath = imagePath?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedPath) ||
                history.Exists(path => string.Equals(path, normalizedPath, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            history.Add(normalizedPath);
        }

        private static void RemovePathFromHistory(List<string> history, string imagePath)
        {
            if (history == null || string.IsNullOrWhiteSpace(imagePath))
            {
                return;
            }

            var normalizedPath = imagePath.Trim();
            history.RemoveAll(path => string.Equals(path, normalizedPath, StringComparison.OrdinalIgnoreCase));
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
