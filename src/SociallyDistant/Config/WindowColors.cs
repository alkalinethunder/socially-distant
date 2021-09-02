using System.Text.Json.Serialization;

namespace SociallyDistant.Config
{
    public class WindowColors
    {
        [JsonPropertyName("panicBorder")]
        public string PanicBorder;
        
        [JsonPropertyName("border")]
        public string Border;
    }
}