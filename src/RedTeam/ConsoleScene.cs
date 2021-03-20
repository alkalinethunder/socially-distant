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
            yield return "the";
            yield return "of";
            yield return "and";
            yield return "to";
            yield return "a";
            yield return "in";
            yield return "is";
            yield return "i";
            yield return "that";
            yield return "it";
            yield return "for";
            yield return "you";
            yield return "was";
            yield return "with";
            yield return "on";
            yield return "as";
            yield return "have";
            yield return "but";
            yield return "be";
            yield return "they";
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