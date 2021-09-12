using System.Text.Json.Serialization;

namespace SociallyDistant.Online.CommunityAnnouncements
{
    public class DrupalRichText
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
        
        [JsonPropertyName("format")]
        public string Format { get; set; }
        
        [JsonPropertyName("processed")]
        public string Processed { get; set; }
        
        [JsonPropertyName("summary")]
        public string Summary { get; set; }
    }
}