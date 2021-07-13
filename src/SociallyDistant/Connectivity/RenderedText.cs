using System.Text.Json.Serialization;

namespace SociallyDistant.Connectivity
{
    public class RenderedText
    {
        [JsonPropertyName("rendered")]
        public string Rendered { get; set; }
    }
}