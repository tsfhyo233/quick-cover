using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Playnite.SDK;
using QuickCover.Services;

namespace QuickCover
{
    public partial class QuickCoverSettingsView : UserControl
    {
        private const string SupportedImageFilter = "Image Files|*.jpg;*.jpeg;*.png;*.webp|All Files|*.*";

        private readonly IPlayniteAPI playniteApi;
        private readonly QuickCoverSettings settings;
        private readonly ImageDownloadService imageDownloadService;
        private readonly List<string> previewTempFilePaths = new List<string>();
        private bool isRefreshing;

        public QuickCoverSettingsView(IPlayniteAPI playniteApi, QuickCoverSettings settings, ImageDownloadService imageDownloadService)
        {
            InitializeComponent();

            this.playniteApi = playniteApi;
            this.settings = settings;
            this.imageDownloadService = imageDownloadService;

            RefreshDisplayedValues();
            Unloaded += QuickCoverSettingsView_Unloaded;
        }

        private void BrowseDefaultCoverImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = SelectImagePath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            settings.DefaultCoverImagePath = selectedPath;
            RefreshDisplayedValues();
        }

        private void ClearDefaultCoverImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            settings.DefaultCoverImagePath = string.Empty;
            RefreshDisplayedValues();
        }

        private void ClearDefaultCoverImageUrlButton_Click(object sender, RoutedEventArgs e)
        {
            settings.DefaultCoverImageUrl = string.Empty;
            RefreshDisplayedValues();
        }

        private void BrowseDefaultBackgroundImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = SelectImagePath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            settings.DefaultBackgroundImagePath = selectedPath;
            RefreshDisplayedValues();
        }

        private void ClearDefaultBackgroundImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            settings.DefaultBackgroundImagePath = string.Empty;
            RefreshDisplayedValues();
        }

        private void ClearDefaultBackgroundImageUrlButton_Click(object sender, RoutedEventArgs e)
        {
            settings.DefaultBackgroundImageUrl = string.Empty;
            RefreshDisplayedValues();
        }

        private void DefaultCoverImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRefreshing)
            {
                return;
            }

            settings.DefaultCoverImageUrl = DefaultCoverImageUrlTextBox.Text?.Trim() ?? string.Empty;
        }

        private void DefaultBackgroundImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRefreshing)
            {
                return;
            }

            settings.DefaultBackgroundImageUrl = DefaultBackgroundImageUrlTextBox.Text?.Trim() ?? string.Empty;
        }

        private void DefaultCoverImageUrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
        }

        private void DefaultBackgroundImageUrlTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            RefreshPreviews();
        }

        private void QuickCoverSettingsView_Unloaded(object sender, RoutedEventArgs e)
        {
            CleanupPreviewTempFiles();
            Unloaded -= QuickCoverSettingsView_Unloaded;
        }

        private string SelectImagePath()
        {
            return playniteApi.Dialogs.SelectFile(SupportedImageFilter);
        }

        private void RefreshDisplayedValues()
        {
            isRefreshing = true;
            try
            {
                DefaultCoverImagePathTextBox.Text = settings.DefaultCoverImagePath ?? string.Empty;
                DefaultCoverImageUrlTextBox.Text = settings.DefaultCoverImageUrl ?? string.Empty;
                DefaultBackgroundImagePathTextBox.Text = settings.DefaultBackgroundImagePath ?? string.Empty;
                DefaultBackgroundImageUrlTextBox.Text = settings.DefaultBackgroundImageUrl ?? string.Empty;
            }
            finally
            {
                isRefreshing = false;
            }

            RefreshPreviews();
        }

        private void RefreshPreviews()
        {
            CleanupPreviewTempFiles();

            SetPreviewImage(
                DefaultCoverPreviewImage,
                DefaultCoverPreviewPlaceholderTextBlock,
                settings.DefaultCoverImagePath,
                settings.DefaultCoverImageUrl);

            SetPreviewImage(
                DefaultBackgroundPreviewImage,
                DefaultBackgroundPreviewPlaceholderTextBlock,
                settings.DefaultBackgroundImagePath,
                settings.DefaultBackgroundImageUrl);
        }

        private void SetPreviewImage(Image imageControl, TextBlock placeholderTextBlock, string imagePath, string imageUrl)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            {
                if (TryLoadPreview(imageControl, imagePath))
                {
                    placeholderTextBlock.Visibility = Visibility.Collapsed;
                    return;
                }
            }

            if (QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                try
                {
                    var tempFilePath = imageDownloadService.DownloadToTempFile(imageUrl);
                    previewTempFilePaths.Add(tempFilePath);

                    if (TryLoadPreview(imageControl, tempFilePath))
                    {
                        placeholderTextBlock.Visibility = Visibility.Collapsed;
                        return;
                    }
                }
                catch
                {
                }

                imageControl.Source = null;
                placeholderTextBlock.Text = "Preview unavailable";
                placeholderTextBlock.Visibility = Visibility.Visible;
                return;
            }

            imageControl.Source = null;
            placeholderTextBlock.Text = "No preview";
            placeholderTextBlock.Visibility = Visibility.Visible;
        }

        private static bool TryLoadPreview(Image imageControl, string imageSource)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imageSource, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                imageControl.Source = bitmap;
                return true;
            }
            catch
            {
                imageControl.Source = null;
                return false;
            }
        }

        private void CleanupPreviewTempFiles()
        {
            foreach (var previewTempFilePath in previewTempFilePaths)
            {
                try
                {
                    if (File.Exists(previewTempFilePath))
                    {
                        File.Delete(previewTempFilePath);
                    }
                }
                catch
                {
                }
            }

            previewTempFilePaths.Clear();
        }
    }
}
