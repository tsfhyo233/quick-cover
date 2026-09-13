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
        private bool isRefreshing;
        private string selectedPresetName;

        public QuickCoverSettingsView(IPlayniteAPI playniteApi, QuickCoverSettings settings, ImageDownloadService imageDownloadService)
        {
            InitializeComponent();

            this.playniteApi = playniteApi;
            this.settings = settings;
            this.imageDownloadService = imageDownloadService;

            RefreshImagePresetControls();
            RefreshDisplayedValues();
        }

        private void ImagePresetsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isRefreshing || !(ImagePresetsComboBox.SelectedItem is string name))
            {
                return;
            }

            var preset = settings.FindImagePreset(name);
            if (preset == null)
            {
                return;
            }

            selectedPresetName = preset.Name;
            settings.UseImagePreset(preset);
            RefreshDisplayedValues();
        }

        private void SaveCurrentImagePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(settings.DefaultCoverImagePath) &&
                string.IsNullOrWhiteSpace(settings.DefaultCoverImageUrl) &&
                string.IsNullOrWhiteSpace(settings.DefaultBackgroundImagePath) &&
                string.IsNullOrWhiteSpace(settings.DefaultBackgroundImageUrl))
            {
                playniteApi.Dialogs.ShowMessage("Configure a cover or background image before saving a preset.", "Quick Cover");
                return;
            }

            var result = playniteApi.Dialogs.SelectString("Enter a preset name.", "Quick Cover", selectedPresetName ?? string.Empty);
            if (result == null || !result.Result)
            {
                return;
            }

            var name = result.SelectedString?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                playniteApi.Dialogs.ShowMessage("Preset name cannot be empty.", "Quick Cover");
                return;
            }

            if (settings.FindImagePreset(name) != null &&
                playniteApi.Dialogs.ShowMessage($"Overwrite preset \"{name}\"?", "Quick Cover",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            if (settings.SaveCurrentAsImagePreset(name))
            {
                selectedPresetName = name;
                RefreshImagePresetControls();
            }
        }

        private void RemoveImagePresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string name)
            {
                settings.RemoveImagePreset(name);
                if (string.Equals(selectedPresetName, name, StringComparison.OrdinalIgnoreCase))
                {
                    selectedPresetName = null;
                }

                RefreshImagePresetControls();
                e.Handled = true;
            }
        }

        private void ClearImagePresetSelection()
        {
            selectedPresetName = null;
            var wasRefreshing = isRefreshing;
            isRefreshing = true;
            try
            {
                ImagePresetsComboBox.SelectedItem = null;
                ImagePresetsComboBox.Text = string.Empty;
            }
            finally
            {
                isRefreshing = wasRefreshing;
            }
        }

        private void RefreshImagePresetControls()
        {
            var wasRefreshing = isRefreshing;
            isRefreshing = true;
            try
            {
                SetImagePresetItems();
            }
            finally
            {
                isRefreshing = wasRefreshing;
            }
        }

        private void SetImagePresetItems()
        {
            var names = new List<string>();
            if (settings.ImagePresets != null)
            {
                foreach (var preset in settings.ImagePresets)
                {
                    if (preset != null)
                    {
                        names.Add(preset.Name);
                    }
                }
            }

            ImagePresetsComboBox.ItemsSource = names;
            ImagePresetsComboBox.SelectedItem = FindPath(names, selectedPresetName);
            ImagePresetsComboBox.Text = selectedPresetName ?? string.Empty;
        }

        private void BrowseDefaultCoverImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = SelectImagePath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            ClearImagePresetSelection();
            settings.UseDefaultCoverImagePath(selectedPath);
            RefreshDisplayedValues();
        }

        private void ClearDefaultCoverImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            ClearImagePresetSelection();
            settings.DefaultCoverImagePath = string.Empty;
            RefreshDisplayedValues();
        }

        private void ClearDefaultCoverImageUrlButton_Click(object sender, RoutedEventArgs e)
        {
            ClearImagePresetSelection();
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

            ClearImagePresetSelection();
            settings.UseDefaultBackgroundImagePath(selectedPath);
            RefreshDisplayedValues();
        }

        private void ClearDefaultBackgroundImagePathButton_Click(object sender, RoutedEventArgs e)
        {
            ClearImagePresetSelection();
            settings.DefaultBackgroundImagePath = string.Empty;
            RefreshDisplayedValues();
        }

        private void ClearDefaultBackgroundImageUrlButton_Click(object sender, RoutedEventArgs e)
        {
            ClearImagePresetSelection();
            settings.DefaultBackgroundImageUrl = string.Empty;
            RefreshDisplayedValues();
        }

        private void DefaultCoverImagePathHistoryComboBox_DropDownClosed(object sender, EventArgs e)
        {
            RefreshPathHistoryControls();
        }

        private void DefaultBackgroundImagePathHistoryComboBox_DropDownClosed(object sender, EventArgs e)
        {
            RefreshPathHistoryControls();
        }

        private void DefaultCoverImagePathHistoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isRefreshing || !(DefaultCoverImagePathHistoryComboBox.SelectedItem is string imagePath))
            {
                return;
            }

            ClearImagePresetSelection();
            settings.UseDefaultCoverImagePath(imagePath);
            DefaultCoverImagePathHistoryComboBox.Text = settings.DefaultCoverImagePath;
            SetLocalPreviewImage(
                DefaultCoverLocalPreviewImage,
                DefaultCoverLocalPreviewPlaceholderTextBlock,
                settings.DefaultCoverImagePath);
        }

        private void DefaultBackgroundImagePathHistoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isRefreshing || !(DefaultBackgroundImagePathHistoryComboBox.SelectedItem is string imagePath))
            {
                return;
            }

            ClearImagePresetSelection();
            settings.UseDefaultBackgroundImagePath(imagePath);
            DefaultBackgroundImagePathHistoryComboBox.Text = settings.DefaultBackgroundImagePath;
            SetLocalPreviewImage(
                DefaultBackgroundLocalPreviewImage,
                DefaultBackgroundLocalPreviewPlaceholderTextBlock,
                settings.DefaultBackgroundImagePath);
        }

        private void RemoveDefaultCoverImagePathHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string imagePath)
            {
                settings.RemoveDefaultCoverImagePathHistory(imagePath);
                RefreshPathHistoryControls();
                e.Handled = true;
            }
        }

        private void RemoveDefaultBackgroundImagePathHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string imagePath)
            {
                settings.RemoveDefaultBackgroundImagePathHistory(imagePath);
                RefreshPathHistoryControls();
                e.Handled = true;
            }
        }

        private void DefaultCoverImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRefreshing)
            {
                return;
            }

            ClearImagePresetSelection();
            settings.DefaultCoverImageUrl = DefaultCoverImageUrlTextBox.Text?.Trim() ?? string.Empty;
        }

        private void DefaultBackgroundImageUrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isRefreshing)
            {
                return;
            }

            ClearImagePresetSelection();
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

        private string SelectImagePath()
        {
            return playniteApi.Dialogs.SelectFile(SupportedImageFilter);
        }

        private void RefreshDisplayedValues()
        {
            isRefreshing = true;
            try
            {
                DefaultCoverImagePathHistoryComboBox.Text = settings.DefaultCoverImagePath ?? string.Empty;
                DefaultCoverImageUrlTextBox.Text = settings.DefaultCoverImageUrl ?? string.Empty;
                DefaultBackgroundImagePathHistoryComboBox.Text = settings.DefaultBackgroundImagePath ?? string.Empty;
                DefaultBackgroundImageUrlTextBox.Text = settings.DefaultBackgroundImageUrl ?? string.Empty;
                SetPathHistoryItems();
            }
            finally
            {
                isRefreshing = false;
            }

            RefreshPreviews();
        }

        private void RefreshPathHistoryControls()
        {
            isRefreshing = true;
            try
            {
                SetPathHistoryItems();
            }
            finally
            {
                isRefreshing = false;
            }
        }

        private void SetPathHistoryItems()
        {
            var coverHistory = new List<string>(
                settings.DefaultCoverImagePathHistory ?? new List<string>());
            var backgroundHistory = new List<string>(
                settings.DefaultBackgroundImagePathHistory ?? new List<string>());

            DefaultCoverImagePathHistoryComboBox.ItemsSource = coverHistory;
            DefaultCoverImagePathHistoryComboBox.SelectedItem = FindPath(
                coverHistory,
                settings.DefaultCoverImagePath);
            DefaultCoverImagePathHistoryComboBox.Text = settings.DefaultCoverImagePath ?? string.Empty;

            DefaultBackgroundImagePathHistoryComboBox.ItemsSource = backgroundHistory;
            DefaultBackgroundImagePathHistoryComboBox.SelectedItem = FindPath(
                backgroundHistory,
                settings.DefaultBackgroundImagePath);
            DefaultBackgroundImagePathHistoryComboBox.Text = settings.DefaultBackgroundImagePath ?? string.Empty;
        }

        private static string FindPath(IEnumerable<string> history, string imagePath)
        {
            if (history == null || string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }

            foreach (var historyPath in history)
            {
                if (string.Equals(historyPath, imagePath, StringComparison.OrdinalIgnoreCase))
                {
                    return historyPath;
                }
            }

            return null;
        }

        private void RefreshPreviews()
        {
            SetLocalPreviewImage(
                DefaultCoverLocalPreviewImage,
                DefaultCoverLocalPreviewPlaceholderTextBlock,
                settings.DefaultCoverImagePath);

            SetUrlPreviewImage(
                DefaultCoverUrlPreviewImage,
                DefaultCoverUrlPreviewPlaceholderTextBlock,
                settings.DefaultCoverImageUrl);

            SetLocalPreviewImage(
                DefaultBackgroundLocalPreviewImage,
                DefaultBackgroundLocalPreviewPlaceholderTextBlock,
                settings.DefaultBackgroundImagePath);

            SetUrlPreviewImage(
                DefaultBackgroundUrlPreviewImage,
                DefaultBackgroundUrlPreviewPlaceholderTextBlock,
                settings.DefaultBackgroundImageUrl);
        }

        private void SetLocalPreviewImage(Image imageControl, TextBlock placeholderTextBlock, string imagePath)
        {
            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath) && TryLoadPreview(imageControl, imagePath))
            {
                placeholderTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            imageControl.Source = null;
            placeholderTextBlock.Text = string.IsNullOrWhiteSpace(imagePath) ? "No preview" : "Preview unavailable";
            placeholderTextBlock.Visibility = Visibility.Visible;
        }

        private void SetUrlPreviewImage(Image imageControl, TextBlock placeholderTextBlock, string imageUrl)
        {
            if (QuickCoverSettings.IsValidHttpUrl(imageUrl))
            {
                try
                {
                    var cachedFilePath = imageDownloadService.GetOrDownloadCachedFile(imageUrl);
                    if (TryLoadPreview(imageControl, cachedFilePath))
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

    }
}
