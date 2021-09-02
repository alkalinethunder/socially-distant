using Thundershock.Gui.Elements;

namespace SociallyDistant.Core
{
    public sealed class BadPropertyEditor : AssetPropertyEditor<object>
    {
        private TextBlock _text = new();
        
        protected override void Build()
        {
            RootElement = _text;
            RootElement.Enabled = false;
        }

        protected override void OnValueChanged()
        {
            _text.Text = Value?.ToString() ?? "<null>";
        }
    }
}