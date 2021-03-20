using Microsoft.Xna.Framework;
using RedTeam.Gui;

namespace RedTeam
{
    public class ConsoleScene : Scene
    {
        private GuiSystem _guiSystem;
        private ConsoleControl _console;

        private string _username = "michael";
        private string _hostname = "redteam-os";
        private string _work = "~";

        protected override void OnLoad()
        {
            _guiSystem = AddComponent<GuiSystem>();
            _console = new ConsoleControl();
            _guiSystem.AddToViewport(_console);
            
            WritePrompt();
        }

        private void WritePrompt()
        {
            _console.Write("{0}@{1}:{2}# ", _username, _hostname, _work);
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (_console.GetLine(out string text))
            {
                _console.WriteLine(text);
                WritePrompt();
            }
        }
    }
}