using System;
using System.Text.Json.Serialization;

namespace SociallyDistant.Connectivity
{
    public class DrupalResponse<T> where T : DrupalAttributes, new()
    {
        [JsonPropertyName("data")]
        public DrupalData<T>[] Data { get; set; }
    }

    public class DrupalData<T> where T : DrupalAttributes, new()
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        
        [JsonPropertyName("attributes")]
        public T Attributes { get; set; }
    }

    public abstract class DrupalAttributes
    {
        [JsonPropertyName("drupal_internal__nid")]
        public int NodeId { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
    }

    public class Article : DrupalAttributes
    {
        [JsonPropertyName("body")]
        public DrupalRichText Body { get; set; }
    }
    
}