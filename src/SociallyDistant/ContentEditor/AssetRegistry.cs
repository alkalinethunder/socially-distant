using System.Collections.Generic;
using SociallyDistant.Core;

namespace SociallyDistant.ContentEditor
{
    public sealed class AssetRegistry
    {
        private Dictionary<AssetInfo, List<IAsset>> _assets = new();

        public IEnumerable<AssetInfo> GetAssetTypes()
        {
            return _assets.Keys;
        }

        public IEnumerable<IAsset> GetAssets(AssetInfo info)
        {
            foreach (var asset in _assets[info])
                yield return asset;
        }
        
        public void Add(AssetInfo info)
        {
            if (!_assets.ContainsKey(info))
                _assets.Add(info, new());
        }
        
        public void Add(AssetInfo info, IAsset asset)
        {
            Add(info);            
            _assets[info].Add(asset);
        }

        public void Remove(IAsset asset)
        {
            foreach (var key in _assets.Keys)
            {
                if (_assets[key].Contains(asset))
                {
                    _assets[key].Remove(asset);
                    return;
                }
            }
        }

        public void Clear(AssetInfo info)
        {
            if (_assets.ContainsKey(info))
            {
                _assets[info].Clear();
                _assets.Remove(info);
            }
        }

        public void ClearAssets()
        {
            foreach (var key in _assets.Keys)
            {
                _assets[key].Clear();
            }
        }
        
        public void Clear()
        {
            _assets.Clear();
        }
    }
}