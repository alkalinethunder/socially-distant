using System.Text.Json.Serialization;

namespace SociallyDistant.Config
{
    public class RedTermCompletions
    {
        [JsonPropertyName("bg")]
        public string Bg;
        
        [JsonPropertyName("text")]
        public string Text;
        
        [JsonPropertyName("textHighlight")]
        public string TextHighlight;
        
        [JsonPropertyName("highlight")]
        public string Highlight;
        
        [JsonPropertyName("border")]
        public string Border;
    }
}