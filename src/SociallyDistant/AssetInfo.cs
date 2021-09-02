using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SociallyDistant.Core.ContentEditors;
using Thundershock;

namespace SociallyDistant.Core
{
    public class AssetInfo
    {
        private Type _customViewType;
        private List<AssetProperty> _props = new();
        
        public bool HasCustomView { get; private set; }
        public bool CanUserCreate { get; private set; }
        public bool AutoCreate { get; private set; }
        public string AutoCreateName { get; private set; }
        public string Name { get; }
        public Type AssetType { get; }
        
        public IEnumerable<AssetProperty> Properties => _props;
        
        private AssetInfo(Type type, string name)
        {
            Name = name;
            AssetType = type;
        }

        public static AssetInfo Create<T>(string name) where T : IAsset, new()
        {
            var r = new AssetInfo(typeof(T), name);
            r.Reflect();
            return r;
        }

        public IAssetView CreateCustomView(IAsset asset)
        {
            if (HasCustomView)
            {
                var view = (IAssetView) Activator.CreateInstance(_customViewType, null);
                view.SelectAsset(asset);

                return view;
            }

            return null;
        }

        public static AssetInfo CreateRuntime(Type type)
        {
            var r = new AssetInfo(type, type.FullName);
            r.Reflect();
            return r;
        }
        
        private void Reflect()
        {
            var userCreateAttrib =
                AssetType.GetCustomAttributes(false).OfType<UserCreatableAttribute>().FirstOrDefault();
            if (userCreateAttrib != null && !userCreateAttrib.IsUserCreateable)
            {
                this.CanUserCreate = false;
            }
            else
            {
                this.CanUserCreate = true;
            }

            var autoCreate = AssetType.GetCustomAttributes(false).OfType<AutoCreateAttribute>().FirstOrDefault();
            if (autoCreate != null)
            {
                this.AutoCreate = true;
                this.AutoCreateName = autoCreate.Name;
            }
            
            foreach (var prop in AssetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetCustomAttributes(true).OfType<EditorHiddenAttribute>().Any())
                    continue;
                
                _props.Add(new AssetProperty(prop));
            }

            var customView = AssetType.GetCustomAttributes(false).OfType<CustomViewAttribute>().FirstOrDefault();

            if (customView != null)
            {
                foreach (var type in ThundershockPlatform.GetAllTypes<IAssetView>())
                {
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        continue;

                    if (type.FullName != customView.Name)
                        continue;

                    HasCustomView = true;
                    _customViewType = type;

                    break;
                }
            }
        }
    }
}