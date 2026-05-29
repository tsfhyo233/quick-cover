using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Models;
using QuickCover.Models;
using QuickCover.Services;

namespace QuickCover.Actions
{
    public class ApplyDefaultImagesAction
    {
        private readonly IPlayniteAPI playniteApi;
        private readonly ImageImportService imageImportService;
        private readonly QuickCoverSettings settings;
        private readonly DefaultImageSourceMode sourceMode;
        private readonly DefaultImageTarget imageTarget;

        public ApplyDefaultImagesAction(IPlayniteAPI playniteApi, ImageImportService imageImportService, QuickCoverSettings settings, DefaultImageSourceMode sourceMode, DefaultImageTarget imageTarget = DefaultImageTarget.Both)
        {
            this.playniteApi = playniteApi;
            this.imageImportService = imageImportService;
            this.settings = settings;
            this.sourceMode = sourceMode;
            this.imageTarget = imageTarget;
        }

        public void Execute(IEnumerable<Game> games)
        {
            var selectedGames = games?.ToList() ?? new List<Game>();
            if (selectedGames.Count == 0)
            {
                ShowNotification("Please select at least one game.", NotificationType.Error);
                return;
            }

            if (!HasAnyConfiguredDefaultSource())
            {
                ShowNotification(GetMissingSourceMessage(), NotificationType.Error);
                return;
            }

            if (!AreConfiguredSourcesValid())
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
                        settings.DefaultCoverImageUrl,
                        settings.DefaultBackgroundImagePath,
                        settings.DefaultBackgroundImageUrl,
                        sourceMode,
                        imageTarget);

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

            if (failedGames > 0)
            {
                ShowNotification(
                    $"Applied defaults to {updatedGames} game(s); failed {failedGames}.",
                    NotificationType.Error);
            }
        }

        private bool HasAnyConfiguredDefaultSource()
        {
            switch (imageTarget)
            {
                case DefaultImageTarget.CoverOnly:
                    return HasConfiguredSource(settings.DefaultCoverImagePath, settings.DefaultCoverImageUrl);
                case DefaultImageTarget.BackgroundOnly:
                    return HasConfiguredSource(settings.DefaultBackgroundImagePath, settings.DefaultBackgroundImageUrl);
                default:
                    return HasConfiguredSource(settings.DefaultCoverImagePath, settings.DefaultCoverImageUrl)
                        || HasConfiguredSource(settings.DefaultBackgroundImagePath, settings.DefaultBackgroundImageUrl);
            }
        }

        private bool AreConfiguredSourcesValid()
        {
            switch (imageTarget)
            {
                case DefaultImageTarget.CoverOnly:
                    return IsConfiguredSourceValid(settings.DefaultCoverImagePath, settings.DefaultCoverImageUrl, "default cover image");
                case DefaultImageTarget.BackgroundOnly:
                    return IsConfiguredSourceValid(settings.DefaultBackgroundImagePath, settings.DefaultBackgroundImageUrl, "default background image");
                default:
                    return IsConfiguredSourceValid(settings.DefaultCoverImagePath, settings.DefaultCoverImageUrl, "default cover image")
                        && IsConfiguredSourceValid(settings.DefaultBackgroundImagePath, settings.DefaultBackgroundImageUrl, "default background image");
            }
        }

        private bool HasConfiguredSource(string imagePath, string imageUrl)
        {
            return sourceMode == DefaultImageSourceMode.UrlOnly
                ? !string.IsNullOrWhiteSpace(imageUrl)
                : !string.IsNullOrWhiteSpace(imagePath) || !string.IsNullOrWhiteSpace(imageUrl);
        }

        private bool IsConfiguredSourceValid(string imagePath, string imageUrl, string imageLabel)
        {
            if (sourceMode == DefaultImageSourceMode.UrlOnly)
            {
                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    return true;
                }

                if (QuickCoverSettings.IsValidHttpUrl(imageUrl))
                {
                    return true;
                }

                ShowNotification($"The configured {imageLabel} URL is invalid.", NotificationType.Error);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(imagePath))
            {
                if (File.Exists(imagePath))
                {
                    return true;
                }

                ShowNotification($"The configured {imageLabel} file was not found.", NotificationType.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return true;
            }

            if (QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                return true;
            }

            ShowNotification($"The configured {imageLabel} URL is invalid.", NotificationType.Error);
            return false;
        }

        private string GetMissingSourceMessage()
        {
            switch (imageTarget)
            {
                case DefaultImageTarget.CoverOnly:
                    return sourceMode == DefaultImageSourceMode.UrlOnly
                        ? "Configure a default cover image URL in Quick Cover settings first."
                        : "Configure a default cover image file or URL in Quick Cover settings first.";
                case DefaultImageTarget.BackgroundOnly:
                    return sourceMode == DefaultImageSourceMode.UrlOnly
                        ? "Configure a default background image URL in Quick Cover settings first."
                        : "Configure a default background image file or URL in Quick Cover settings first.";
                default:
                    return sourceMode == DefaultImageSourceMode.UrlOnly
                        ? "Configure at least one default image URL in Quick Cover settings first."
                        : "Configure at least one default image file or URL in Quick Cover settings first.";
            }
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
