using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SociallyDistant.Utilities;
using Thundershock.Core.Debugging;
using Thundershock.IO;

namespace SociallyDistant.Editor
{
    public sealed class AssetRegistry
    {
        public static readonly byte[] AssetMapHeaderId = Encoding.UTF8.GetBytes("80710a06");
        
        private List<ImageAsset> _images = new();
        private Dictionary<AssetInfo, List<IAsset>> _assets = new();
        private List<IAsset> _dirty = new();
        private Dictionary<IAsset, string> _names = new();
        
        public void ClearDirty() => _dirty.Clear();

        public bool HasDirty => _dirty.Any();
        
        public bool IsDirty(IAsset asset)
            => _dirty.Contains(asset);

        public IEnumerable<T> OfType<T>() where T : IAsset
        {
            foreach (var assetList in _assets.Values)
            {
                foreach (var asset in assetList.OfType<T>())
                {
                    yield return asset;
                }
            }
        }
        
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

        public IEnumerable<IAsset> GetAssets()
        {
            foreach (var key in _assets.Keys)
            foreach (var ass in GetAssets(key))
                yield return ass;
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

        public static AssetRegistry Restore(FileSystem fs)
        {
            var registry = new AssetRegistry();

            // soooo first let's check to make sure assets.map exists.
            if (!fs.FileExists("/assets.map"))
                throw new InvalidOperationException(
                    "Bad world.pak, missing the world assets.map file in the pak root.");

            using var stream = fs.OpenFile("/assets.map");
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            
            // Read the magic header.
            var magic = reader.ReadBytes(AssetMapHeaderId.Length);
            if (!magic.SequenceEqual(AssetMapHeaderId))
                throw new InvalidOperationException("Corrupt assets.map file.");
            
            // How many assets are there?
            var assetsCount = reader.ReadInt32();
            
            // For signature checking
            using var sha256 = SHA256.Create();

            // Keep going until we run out.
            while (assetsCount > 0)
            {
                assetsCount--;
                
                // Read the first bit of metadata.
                var id = reader.ReadString();
                var name = reader.ReadString();
                var typeName = reader.ReadString();
                var path = reader.ReadString();
                
                // Hash what we just loaded.
                var infoCipher = Encoding.UTF8.GetBytes(id + name + typeName + path);
                var infoHash = sha256.ComputeHash(infoCipher);
                
                // Read the info hash from the map file.
                var mapHashLength = reader.ReadInt32();
                var mapHash = reader.ReadBytes(mapHashLength);
                
                // Check if they match - if not, log it to the thundershock console and skip the asset.
                if (!mapHash.SequenceEqual(infoHash))
                {
                    Logger.GetLogger()
                        .Log(
                            "ASSET INFO VERIFICATION FAILURE on asset " + id +
                            ". Possibly tampered asset.map. Skipping the asset.", LogLevel.Error);
                    continue;
                }
                
                // Next we're going to check the asset path to see if it exists.
                if (!fs.FileExists(path))
                {
                    Logger.GetLogger().Log("Asset " + id + " is missing from the world.pak - skipping the asset.",
                        LogLevel.Error);
                    continue;
                }
                
                // Now we're going to load the JSON data.
                var json = fs.ReadAllText(path);
                
                // Hash the json and make sure it matches what the map sees.
                var jsonHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));

                var mapJsonLength = reader.ReadInt32();
                var mapJsonHash = reader.ReadBytes(mapJsonLength);

                if (!jsonHash.SequenceEqual(mapJsonHash))
                {
                    Logger.GetLogger()
                        .Log(
                            "ASSET CONTENTS VERIFICATION FAILED for asset " + id +
                            ". The asset file has been tampered with. Skipping the asset.", LogLevel.Error);
                    continue;
                }
                
                // Now we must find the type.
                var type = GameHelpers.FindType(typeName);
                
                // Null-check it.
                if (type == null)
                {
                    Logger.GetLogger().Log("Missing necessary C# assembly for loading world asset " + id +
                                           " of type " + typeName, LogLevel.Error);
                    continue;
                }
                
                // Deserialize the JSON asset.
                var assetData = (IAsset) JsonSerializer.Deserialize(json, type);
                
                // Get (or create) asset info for the asset.
                var keys = registry._assets.Keys.ToArray();
                var info = keys.FirstOrDefault(x => x.Name == typeName);
                if (info == null)
                {
                    info = AssetInfo.CreateRuntime(type);
                }

                // Add the asset to the registry.
                registry.Add(info, assetData);
            }

            return registry;
        }
    }
}