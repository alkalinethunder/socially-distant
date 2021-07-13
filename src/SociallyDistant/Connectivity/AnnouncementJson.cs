using System;
using System.Text.Json.Serialization;

namespace SociallyDistant.Connectivity
{
    public class AnnouncementJson
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("link")]
        public string Link { get; set; }
        
        [JsonPropertyName("title")]
        public RenderedText Title { get; set; } = new();
        
        [JsonPropertyName("excerpt")]
        public RenderedText Excerpt { get; set; } = new();
        
        [JsonPropertyName("featured_media")]
        public int FeaturedMedia { get; set; }
    }
}