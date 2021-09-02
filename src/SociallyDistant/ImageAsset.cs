using Thundershock.Core.Rendering;

namespace SociallyDistant
{
    public class ImageAsset
    {
        public Texture2D Texture { get; }
        public string Path { get; }

        public ImageAsset(Texture2D texture, string path)
        {
            Texture = texture;
            Path = path;
        }
    }
}