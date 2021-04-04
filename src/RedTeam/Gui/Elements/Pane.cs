using Thundershock.Gui.Elements;

namespace RedTeam.Gui.Elements
{
    public class Pane : Element
    {
        private PaneLayout _layout;
        private Panel _content;

        public ElementCollection Content => _content.Children;
        
        public Pane(PaneLayout layout, Panel content)
        {
            _layout = layout;
            _content = content;

            Children.Add(_layout);
        }
    }
}