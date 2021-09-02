using System.Text.Json.Serialization;

namespace SociallyDistant.Core.Config
{
    public class RedTermBackground
    {
        [JsonPropertyName("image")]
        public string Image;
        
        [JsonPropertyName("opacity")]
        public float Opacity = 1;
    }
}