using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Playnite.SDK;

namespace QuickCover
{
    public partial class QuickCoverSettingsView : UserControl
    {
        private const string SupportedImageFilter = "Image Files|*.jpg;*.jpeg;*.png;*.webp|All Files|*.*";

        private readonly IPlayniteAPI playniteApi;
        private readonly QuickCoverSettings settings;

        public QuickCoverSettingsView(IPlayniteAPI playniteApi, QuickCoverSettings settings)
        {
            InitializeComponent();

            this.playniteApi = playniteApi;
            this.settings = settings;

            RefreshDisplayedPaths();
        }

        private void BrowseDefaultCoverImagePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedPath = SelectImagePath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            settings.DefaultCoverImagePath = selectedPath;
            RefreshDisplayedPaths();
        }

        private void ClearDefaultCoverImagePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settings.DefaultCoverImagePath = string.Empty;
            RefreshDisplayedPaths();
        }

        private void BrowseDefaultBackgroundImagePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var selectedPath = SelectImagePath();
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            settings.DefaultBackgroundImagePath = selectedPath;
            RefreshDisplayedPaths();
        }

        private void ClearDefaultBackgroundImagePathButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            settings.DefaultBackgroundImagePath = string.Empty;
            RefreshDisplayedPaths();
        }

        private string SelectImagePath()
        {
            return playniteApi.Dialogs.SelectFile(SupportedImageFilter);
        }

        private void RefreshDisplayedPaths()
        {
            DefaultCoverImagePathTextBox.Text = settings.DefaultCoverImagePath ?? string.Empty;
            DefaultBackgroundImagePathTextBox.Text = settings.DefaultBackgroundImagePath ?? string.Empty;

            SetPreviewImage(DefaultCoverPreviewImage, DefaultCoverPreviewPlaceholderTextBlock, settings.DefaultCoverImagePath);
            SetPreviewImage(DefaultBackgroundPreviewImage, DefaultBackgroundPreviewPlaceholderTextBlock, settings.DefaultBackgroundImagePath);
        }

        private static void SetPreviewImage(Image imageControl, TextBlock placeholderTextBlock, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                imageControl.Source = null;
                placeholderTextBlock.Visibility = System.Windows.Visibility.Visible;
                return;
            }

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                imageControl.Source = bitmap;
                placeholderTextBlock.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch
            {
                imageControl.Source = null;
                placeholderTextBlock.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
