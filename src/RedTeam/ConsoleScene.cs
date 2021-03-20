using Microsoft.Xna.Framework;
using RedTeam.Gui;
using System.Collections.Generic;

namespace RedTeam
{
    public class ConsoleScene : Scene, IAutoCompleteSource
    {
        private GuiSystem _guiSystem;
        private ConsoleControl _console;

        private string _username = "michael";
        private string _hostname = "redteam-os";
        private string _work = "~";

        public IEnumerable<string> GetCompletions()
        {
            yield return "foo";
            yield return "bar";
            yield return "test";
            yield return "completion";
            yield return "whew";
        }
        
        protected override void OnLoad()
        {
            _guiSystem = AddComponent<GuiSystem>();
            _console = new ConsoleControl();
            _guiSystem.AddToViewport(_console);

            _console.AutoCompleteSource = this;
            
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