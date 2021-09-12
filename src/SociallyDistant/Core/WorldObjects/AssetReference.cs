using System;
using System.Text.Json.Serialization;
using SociallyDistant.Editor;

namespace SociallyDistant.Core.WorldObjects
{
    public sealed class AssetReference<T> : IAssetReference
        where T : IAsset, new()
    {
        public Guid Id { get; set; }
        
        [JsonIgnore]
        public Type AssetType => typeof(T);
    }
}