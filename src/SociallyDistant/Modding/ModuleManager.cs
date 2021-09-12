using System;
using System.Collections.Generic;
using SociallyDistant.Editor;
using Thundershock;

namespace SociallyDistant.Modding
{
    public static class ModuleManager
    {
        private static List<Module> _modules = new();
        
        public static void Initialize()
        {
            _modules.Clear();
            foreach (var modType in ThundershockPlatform.GetAllTypes<Module>())
            {
                var mod = (Module) Activator.CreateInstance(modType, null);
                _modules.Add(mod);
            }
        }

        public static IEnumerable<AssetInfo> GetAssetTypes()
        {
            foreach (var mod in _modules)
            {
                foreach (var assetType in mod.GetAssetTypes())
                {
                    yield return assetType;
                }
            }
        }
    }
}