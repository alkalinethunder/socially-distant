using SociallyDistant.Core.Windowing;
using Thundershock.Core;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core.Gui.Elements
{
    public class Pane : LayoutElement
    {
        private IPaneLayout _layout;
        private Panel _content;

        public ElementCollection Content => _content.Children;

        public WindowManager WindowManager => _layout.WindowManager;
        
        public Color BorderColor
        {
            get => _layout.Color;
            set => _layout.Color = value;
        }
        
        public Pane(IPaneLayout layout, Panel content)
        {
            _layout = layout;
            _content = content;

            Children.Add(_layout as Element);
        }
    }
}