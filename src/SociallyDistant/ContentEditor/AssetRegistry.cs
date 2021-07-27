using System.Collections.Generic;
using System.Linq;
using SociallyDistant.Core;

namespace SociallyDistant.ContentEditor
{
    public sealed class AssetRegistry
    {
        private List<ImageAsset> _images = new();
        private Dictionary<AssetInfo, List<IAsset>> _assets = new();
        private List<IAsset> _dirty = new();
        private Dictionary<IAsset, string> _names = new();
        
        public void ClearDirty() => _dirty.Clear();

        public bool HasDirty => _dirty.Any();
        
        public bool IsDirty(IAsset asset)
            => _dirty.Contains(asset);

        public IEnumerable<ImageAsset> Images => _images;
        
        public void SetDirty(IAsset asset)
        {
            if (!_dirty.Contains(asset))
                _dirty.Add(asset);
        }

        public void AddImage(ImageAsset imageAsset)
        {
            _images.Add(imageAsset);
        }
        
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

            _names.Add(asset, asset.Name);
        }

        public bool CheckName(IAsset asset)
        {
            if (_names[asset] != asset.Name)
            {
                _names[asset] = asset.Name;
                return true;
            }

            return false;
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

            if (IsDirty(asset))
                _dirty.Remove(asset);

            _names.Remove(asset);
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
            ClearDirty();
            
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