using System;
using System.Text.Json.Serialization;

namespace SociallyDistant.Core.ContentEditors
{
    public interface IAssetReference
    {
        public Guid Id { get; set; }
        
        [JsonIgnore]
        public Type AssetType { get; }
    }
}