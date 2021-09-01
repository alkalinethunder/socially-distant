using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using SociallyDistant.ContentEditor;
using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.AssetPropertyEditors
{
    [PropertyEditor(typeof(IAssetReference))]
    public class AssetSelectorDropdown : AssetPropertyEditor<IAssetReference>
    {
        private Stacker _mainStacker = new();
        private Button _select = new();
        private Button _reset = new();
        private TextBlock _name = new();
        private Panel _overlay = new();
        private Panel _dropdown = new();
        private ScrollPanel _scroller = new();
        private Stacker _assetList = new();
        
        
        protected override void Build()
        {
            _mainStacker.Children.Add(_name);
            _mainStacker.Children.Add(_reset);
            _mainStacker.Children.Add(_select);

            _mainStacker.Direction = StackDirection.Horizontal;

            _reset.ToolTip = "Remove the current asset reference from this asset property.";
            _reset.Text = "Remove";
            _select.Text = "Select";
            
            _reset.MouseUp += ResetOnMouseUp;
            _select.MouseUp += SelectOnMouseUp;

            this.UpdateState();

            _overlay.BackColor = Color.Transparent;

            _scroller.Children.Add(_assetList);
            _dropdown.Children.Add(_scroller);

            _overlay.IsInteractable = true;
            _dropdown.IsInteractable = true;
            _scroller.IsInteractable = true;

            RootElement = _mainStacker;
        }

        private void UpdateState()
        {
            if (Value == null)
            {
                Value = (IAssetReference) Activator.CreateInstance(this.Property.PropertyType, null);
            }
            
            if (Value.Id == default)
            {
                _name.Text = "< Nothing selected >";
                _reset.Visibility = Visibility.Collapsed;
            }
            else
            {
                var id = Value.Id;
                var type = Value.AssetType;

                var assets = ContentController.GetAssetsOfType(type).ToArray();

                var asset = assets.FirstOrDefault(Index => Index.Id == id);

                if (asset != null)
                {
                    _name.Text = asset.Name;
                    _reset.Visibility = Visibility.Visible;
                }
                else
                {
                    _reset.Visibility = Visibility.Visible;
                    _name.Text = "< Invalid asset reference >";
                }
            }
        }

        private void SelectOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                _mainStacker.GuiSystem.AddToViewport(_overlay);
                _mainStacker.GuiSystem.AddToViewport(_dropdown);

                _dropdown.ViewportAnchor = FreePanel.CanvasAnchor.TopLeft;
                _dropdown.ViewportPosition =
                    new Vector2(_mainStacker.BoundingBox.Left, _mainStacker.BoundingBox.Bottom);

                this.UpdateAssetList();
            }
        }

        private void UpdateAssetList()
        {
            _assetList.Children.Clear();

            foreach (var asset in ContentController.GetAssetsOfType(Value.AssetType))
            {
                var btn = new AdvancedButton();
                var hStack = new Stacker();
                var vStack = new Stacker();
                var icon = new Picture();
                var title = new TextBlock();
                var text = new TextBlock();

                btn.IsInteractable = true;
                
                title.Text = asset.Name;
                text.Text = Value.AssetType.ToString();

                icon.ImageMode = ImageMode.Rounded;

                icon.FixedWidth = 32;
                icon.FixedHeight = 32;
                
                vStack.Children.Add(title);
                vStack.Children.Add(text);
                hStack.Children.Add(icon);
                hStack.Children.Add(vStack);
                hStack.Direction = StackDirection.Horizontal;

                btn.Children.Add(hStack);

                hStack.Padding = 5;
                icon.Padding = new Padding(0, 0, 3, 0);

                icon.VerticalAlignment = VerticalAlignment.Center;
                vStack.VerticalAlignment = VerticalAlignment.Center;

                btn.MouseUp += (o, a) =>
                {
                    if (a.Button == MouseButton.Primary)
                    {
                        this.Value.Id = asset.Id;
                        this.NotifyValueChanged(this.Value);
                        this.UpdateState();
                        _overlay.RemoveFromParent();
                        _dropdown.RemoveFromParent();
                    }
                };

                _assetList.Children.Add(btn);

                btn.Padding = 2;
            }
        }
        
        private void ResetOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                this.Value.Id = default;
                NotifyValueChanged(Value);
                UpdateState();
            }
        }
    }
}