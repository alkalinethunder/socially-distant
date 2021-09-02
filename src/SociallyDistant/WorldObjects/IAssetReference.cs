using System;
using System.Text.Json.Serialization;

namespace SociallyDistant.WorldObjects
{
    public interface IAssetReference
    {
        public Guid Id { get; set; }
        
        [JsonIgnore]
        public Type AssetType { get; }
    }
}