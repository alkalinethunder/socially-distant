using RedTeam.Gui;

namespace RedTeam
{
    public class ConsoleScene : Scene
    {
        private GuiSystem _guiSystem;
        
        protected override void OnLoad()
        {
            _guiSystem = AddComponent<GuiSystem>();
        }
    }
}