using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using QuickCover.Services;

namespace QuickCover.Actions
{
    public class ApplyDefaultImagesAction
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ImageImportService imageImportService;
        private readonly QuickCoverSettings settings;

        public ApplyDefaultImagesAction(IPlayniteAPI playniteApi, ImageImportService imageImportService, QuickCoverSettings settings)
        {
            this.playniteApi = playniteApi;
            this.imageImportService = imageImportService;
            this.settings = settings;
        }

        public void Execute(IEnumerable<Game> games)
        {
            var selectedGames = games?.ToList() ?? new List<Game>();
            if (selectedGames.Count == 0)
            {
                ShowNotification("Please select at least one game.", NotificationType.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.DefaultCoverImagePath) && string.IsNullOrWhiteSpace(settings.DefaultBackgroundImagePath))
            {
                ShowNotification("Configure at least one default image in Quick Cover settings first.", NotificationType.Error);
                return;
            }

            if (!IsConfiguredPathValid(settings.DefaultCoverImagePath, "default cover image") ||
                !IsConfiguredPathValid(settings.DefaultBackgroundImagePath, "default background image"))
            {
                return;
            }

            var updatedGames = 0;
            var failedGames = 0;

            foreach (var game in selectedGames)
            {
                try
                {
                    var changed = imageImportService.ApplyDefaultImages(
                        game,
                        settings.DefaultCoverImagePath,
                        settings.DefaultBackgroundImagePath);

                    if (changed)
                    {
                        updatedGames++;
                    }
                }
                catch
                {
                    failedGames++;
                }
            }

            var notificationType = failedGames == 0 ? NotificationType.Info : NotificationType.Error;
            ShowNotification(
                $"Applied defaults to {updatedGames} game(s); failed {failedGames}.",
                notificationType);
        }

        private bool IsConfiguredPathValid(string imagePath, string imageLabel)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return true;
            }

            if (File.Exists(imagePath))
            {
                return true;
            }

            ShowNotification($"The configured {imageLabel} file was not found.", NotificationType.Error);
            return false;
        }

        private void ShowNotification(string message, NotificationType notificationType)
        {
            playniteApi.Notifications.Add(new NotificationMessage(
                $"QuickCover_{Guid.NewGuid():N}",
                message,
                notificationType));
        }
    }
}
