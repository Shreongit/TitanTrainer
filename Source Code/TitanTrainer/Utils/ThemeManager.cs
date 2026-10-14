using System.Drawing;

namespace TitanTrainer.Utils
{
    /// <summary>
    /// Small theme manager for storing colors and theme choices.
    /// Expandable to integrate with LemonUI styling later.
    /// </summary>
    public sealed class ThemeManager
    {
        public Color PrimaryColor { get; private set; } = Color.FromArgb(30, 144, 255);
        public Color AccentColor { get; private set; } = Color.FromArgb(255, 215, 0);
        public Color BackgroundColor { get; private set; } = Color.FromArgb(20, 20, 20);
        public Color TextColor { get; private set; } = Color.White;

        public void ApplyTheme(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return;
            switch (name.ToLowerInvariant())
            {
                case "dark":
                    PrimaryColor = Color.FromArgb(20, 20, 20);
                    AccentColor = Color.FromArgb(255, 140, 0);
                    TextColor = Color.White;
                    break;
                case "blue":
                    PrimaryColor = Color.FromArgb(30, 144, 255);
                    AccentColor = Color.FromArgb(255, 215, 0);
                    TextColor = Color.White;
                    break;
                default:
                    break;
            }
        }
    }
}