using System;
using System.Collections.Generic;
using System.Linq;
using RedTeam.Core;
using Thundershock;
using Thundershock.Debugging;
using Thundershock.Gui.Elements.Console;

namespace RedTeam.ContentEditor
{
    public class EditorConsole : GlobalComponent, ILogOutput
    {
        private ConsoleControl _console = null;
        private Queue<string> _preQueue = new Queue<string>();

        public void SetConsole(ConsoleControl console)
        {
            _console = console;
            while (_preQueue.Any())
                _console.WriteLine(_preQueue.Dequeue());
        }

        private void WriteInternal(string message)
        {
            if (_console != null)
            {
                _console.WriteLine(message);
            }
            else
            {
                _preQueue.Enqueue(message);
            }
        }
        
        public void Log(string message, LogLevel logLevel)
        {
            var color = logLevel switch
            {
                LogLevel.Info => "",
                LogLevel.Message => "#f",
                LogLevel.Warning => "#e",
                LogLevel.Error => "#c",
                LogLevel.Fatal => "#c&b",
                LogLevel.Trace => "#7&i",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
            };

            WriteInternal($"{color}{message}&0");
        }
    }
}