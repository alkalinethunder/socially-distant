using System;
using System.Text.Json.Serialization;

namespace SociallyDistant.Online.CommunityAnnouncements
{
    public class AnnouncementObject
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("uid")]
        public string Username { get; set; }
        
        [JsonPropertyName("created")]
        public string CreatedRaw { get; set; }

        [JsonIgnore]
        public DateTime Created => DateTime.Parse(CreatedRaw.Substring(0, CreatedRaw.Length - 5));
        
        [JsonPropertyName("body")]
        public string Content { get; set; }
        
        [JsonPropertyName("field_image")]
        public string FeaturedImage { get; set; }
        
        [JsonPropertyName("view_node")]
        public string Link { get; set; }
    }
}