using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.Xna.Framework;

namespace RedTeam
{
    public class Shell : SceneComponent, IAutoCompleteSource
    {
        private IConsole _console;
        private List<Builtin> _builtins = new List<Builtin>();

        public void RegisterBuiltin(string name, string desc, Action<IConsole, string, string[]> action)
        {
            var builtin = _builtins.FirstOrDefault(x => x.Name == name);
            if (builtin != null)
            {
                builtin.Description = desc;
                builtin.Action = action;
            }
            else
            {
                builtin = new Builtin
                {
                    Name = name,
                    Description = desc,
                    Action = action
                };
                _builtins.Add(builtin);
            }
        }

        private void WritePrompt()
        {
            _console.Write("# ");
        }
        
        public void RegisterBuiltin(string name, string desc, Action action)
        {
            RegisterBuiltin(name, desc, (console, cmd, args) => action());
        }
        
        public IEnumerable<string> GetCompletions()
        {
            foreach (var builtin in _builtins)
            {
                yield return builtin.Name;
            }
        }

        public Shell(IConsole console)
        {
            _console = console;
            _console.AutoCompleteSource = this;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            RegisterBuiltin("clear", "Clear the screen", _console.Clear);
            RegisterBuiltin("echo", "Write text to the screen", Echo);
            
            WritePrompt();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_console.GetLine(out string line))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    ProcessCommand(line);
                }
                else
                {
                    WritePrompt();
                }
            }
        }

        private void Echo(IConsole console, string name, string[] args)
        {
            console.WriteLine(string.Join(" ", args));
        }

        private string[] BreakLine(string commandLine)
        {
            var inQuote = false;
            var inEscape = false;
            var quote = '"';
            var escape = '\\';
            var words = new List<string>();
            var word = "";

            for (var i = 0; i <= commandLine.Length; i++)
            {
                if (i == commandLine.Length)
                {
                    if (inQuote)
                        throw new SyntaxErrorException("unterminated string");

                    if (inEscape)
                        throw new SyntaxErrorException("unexpected escape sequence");

                    if (!string.IsNullOrEmpty(word))
                    {
                        words.Add(word);
                        word = string.Empty;
                    }
                }
                else
                {
                    var ch = commandLine[i];

                    if (inEscape)
                    {
                        word += ch;
                        inEscape = false;
                        continue;
                    }

                    if (ch == escape)
                    {
                        inEscape = true;
                        continue;
                    }

                    if (inQuote)
                    {
                        if (ch == quote)
                        {
                            inQuote = false;
                            continue;
                        }

                        word += ch;
                    }
                    else
                    {
                        if (ch == quote)
                        {
                            inQuote = true;
                            continue;
                        }

                        if (char.IsWhiteSpace(ch))
                        {
                            if (!string.IsNullOrEmpty(word))
                            {
                                words.Add(word);
                                word = string.Empty;
                            }

                            continue;
                        }

                        word += ch;
                    }
                }
            }
            
            return words.ToArray();
        }

        private bool ProcessBuiltin(string name, string[] args)
        {
            var builtin = _builtins.FirstOrDefault(x => x.Name == name);
            if (builtin != null)
            {
                if (builtin.Action != null)
                {
                    builtin.Action(_console, name, args);
                    return true;
                }
            }

            return false;
        }
        
        private void ProcessCommand(string commandLine)
        {
            try
            {
                var words = BreakLine(commandLine);

                if (words.Any())
                {
                    var name = words.First();
                    var args = words.Skip(1).ToArray();

                    if (ProcessBuiltin(name, args))
                    {
                        WritePrompt();
                    }
                    else
                    {
                        _console.WriteLine("sh: {0}: Command not found.", name);
                    }
                }
                else
                {
                    WritePrompt();
                }
            }
            catch (SyntaxErrorException ex)
            {
                _console.WriteLine("sh: error: {0}", ex.Message);
                WritePrompt();
            }
        }
        
        private class Builtin
        {
            public string Name;
            public string Description;
            public Action<IConsole, string, string[]> Action;
        }
    }
}