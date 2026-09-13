namespace QuickCover.Models
{
    public class QuickCoverImagePreset
    {
        public string Name { get; set; } = string.Empty;

        public string CoverImagePath { get; set; } = string.Empty;

        public string CoverImageUrl { get; set; } = string.Empty;

        public string BackgroundImagePath { get; set; } = string.Empty;

        public string BackgroundImageUrl { get; set; } = string.Empty;

        public bool HasImageSource()
        {
            return !string.IsNullOrWhiteSpace(CoverImagePath) ||
                !string.IsNullOrWhiteSpace(CoverImageUrl) ||
                !string.IsNullOrWhiteSpace(BackgroundImagePath) ||
                !string.IsNullOrWhiteSpace(BackgroundImageUrl);
        }

        public QuickCoverImagePreset Clone()
        {
            return new QuickCoverImagePreset
            {
                Name = Name ?? string.Empty,
                CoverImagePath = CoverImagePath ?? string.Empty,
                CoverImageUrl = CoverImageUrl ?? string.Empty,
                BackgroundImagePath = BackgroundImagePath ?? string.Empty,
                BackgroundImageUrl = BackgroundImageUrl ?? string.Empty
            };
        }
    }
}
