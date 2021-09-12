namespace SociallyDistant.Core.Config
{
    public class RedConfig
    {
        public string RedTermPalette;
        public bool SkipIntro = false;
        public int ConsoleFontSize = 0;
        public bool ShowWhatsNew = true;
        public WallpaperSettings WallpaperSettings = new();
    }

    public class WallpaperSettings
    {
        public WallpaperType Type;
        public string Color = "#081206";
        public string WallpaperPath;
        public bool VideoHasAudio;
    }

    public enum WallpaperType
    {
        SolidColor,
        Image,
        Video,
        AudioVisualizer
    }
    
}