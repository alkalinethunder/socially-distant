using System.Linq;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editor.PropertyEditors
{
    [PropertyEditor(typeof(ImageAssetReference))]
    public class ImagePropertyEditor : AssetPropertyEditor<ImageAssetReference>
    {
        private Stacker _stacker = new();
        private Picture _picture = new();
        private WrapPanel _buttonWrapper = new();
        private Button _chooseButton = new();
        private Button _removeButton = new();
        
        protected override void Build()
        {
            _chooseButton.Text = "Choose image";
            _removeButton.Text = "Remove";

            _stacker.Direction = StackDirection.Horizontal;

            _chooseButton.VerticalAlignment = VerticalAlignment.Center;
            _removeButton.VerticalAlignment = VerticalAlignment.Center;
            
            _buttonWrapper.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _stacker.Children.Add(_picture);
            _stacker.Children.Add(_buttonWrapper);
            
            _buttonWrapper.Children.Add(_chooseButton);
            _buttonWrapper.Children.Add(_removeButton);

            _picture.FixedWidth = 80;
            _picture.FixedHeight = 45;
            
            _removeButton.MouseUp += RemoveButtonOnMouseUp;
            _chooseButton.MouseUp += ChooseButtonOnMouseUp;

            this.RootElement = _stacker;
        }

        private void ChooseButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                ContentController.AskForImage("Select image for " + Property.Name, (img) =>
                {
                    var r = new ImageAssetReference();
                    r.Path = img.Path;
                    NotifyValueChanged(r);

                    _picture.Image = img.Texture;
                });
            }
        }

        private void RemoveButtonOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                _picture.Image = null;
                NotifyValueChanged(null);
            }
        }

        protected override void OnValueChanged()
        {
            if (this.Value != null)
            {
                var img = ContentController.Images.FirstOrDefault(x => x.Path == this.Value.Path);
                if (img != null)
                {
                    _picture.Image = img.Texture;
                }
            }
        }
    }
}