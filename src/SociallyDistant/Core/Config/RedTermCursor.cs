using System.Text.Json.Serialization;

namespace SociallyDistant.Core.Config
{
    public class RedTermCursor
    {
        [JsonPropertyName("bg")]
        public string Bg;
        
        [JsonPropertyName("fg")]
        public string Fg;
    }
}