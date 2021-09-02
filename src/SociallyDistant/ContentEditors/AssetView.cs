using System;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core.ContentEditors
{
    public interface IAssetView
    {
        IAsset Asset { get; }
        event EventHandler AssetChanged;
        void SelectAsset(IAsset asset);
        Element RootElement { get; }
    }
    
    public abstract class AssetView<T> : ContentElement, IAssetView where T : IAsset, new()
    {
        private T _asset;

        Element IAssetView.RootElement => this;
        IAsset IAssetView.Asset => _asset;
        
        public event EventHandler AssetChanged;

        protected T Asset => _asset;
        
        public void SelectAsset(IAsset asset)
        {
            var type = asset.GetType();

            if (!typeof(T).IsAssignableFrom(type))
                throw new InvalidOperationException("Invalid asset type.");
            
            _asset = (T) asset;
            
            OnAssetSelected();
        }

        protected virtual void OnAssetSelected()
        {
            
        }
        
        protected void NotifyAssetChanged()
        {
            AssetChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}