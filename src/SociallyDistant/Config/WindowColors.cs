using System.Text.Json.Serialization;

namespace SociallyDistant.Core.Config
{
    public class WindowColors
    {
        [JsonPropertyName("panicBorder")]
        public string PanicBorder;
        
        [JsonPropertyName("border")]
        public string Border;
    }
}