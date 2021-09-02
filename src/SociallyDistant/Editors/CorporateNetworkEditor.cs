using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.WorldObjects;
using Thundershock.Gui.Elements;
using Thundershock.Core;

namespace SociallyDistant.Editors
{
    public class CorporateNetworkEditor : AssetView<CompanyData>
    {
        private Stacker _mainStacker = new();
        private FreePanel _canvas = new();
        private TextBlock _companyName = new();
        private TextBlock _stats = new();
        private Button _addPC = new();
        private Stacker _header = new();
        private Stacker _headerVert = new();

        public CorporateNetworkEditor()
        {
            _header.Direction = StackDirection.Horizontal;
            _canvas.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _companyName.ForeColor = Color.Cyan;
            _companyName.Properties.SetValue(FontStyle.Heading2);
            
            _headerVert.Children.Add(_companyName);
            _headerVert.Children.Add(_stats);
            
            _header.Children.Add(_headerVert);
            _header.Children.Add(_addPC);
            
            _mainStacker.Children.Add(_header);
            _mainStacker.Children.Add(_canvas);

            Children.Add(_mainStacker);
        }
        
        protected override void OnAssetSelected()
        {
            _companyName.Text = Asset.Name;
        }
    }
}