using System.Collections.Generic;
using System;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.Xna.Framework;
using RedTeam.IO;

namespace RedTeam
{
    public class Shell : SceneComponent, IAutoCompleteSource
    {
        private List<string> _completions = new List<string>();
        private IConsole _console;
        private List<Builtin> _builtins = new List<Builtin>();
        private FileSystem _fs;
        private string _work = "/";
        
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

        private void UpdateCompletions()
        {
            _completions.Clear();
            
            // built-ins
            foreach (var builtin in _builtins)
                _completions.Add(builtin.Name);
            
            // files in the working directory.
            foreach (var dir in _fs.GetDirectories(_work))
            {
                var name = PathUtils.GetFileName(dir);

                _completions.Add($".{PathUtils.Separator}{name.Replace(" ", "\\ ")}");
                _completions.Add(name.Replace(" ", "\\ "));

                _completions.Add($"\"{name}\"");
                _completions.Add($"\".{PathUtils.Separator}{name}\"");
            }
            
            foreach (var dir in _fs.GetFiles(_work))
            {
                var name = PathUtils.GetFileName(dir);

                _completions.Add($".{PathUtils.Separator}{name.Replace(" ", "\\ ")}");
                _completions.Add(name.Replace(" ", "\\ "));

                _completions.Add($"\"{name}\"");
                _completions.Add($"\".{PathUtils.Separator}{name}\"");
            }
        }
        
        private void WritePrompt()
        {
            _console.Write("{0}# ", _work);
        }
        
        public void RegisterBuiltin(string name, string desc, Action action)
        {
            RegisterBuiltin(name, desc, (console, cmd, args) => action());
        }
        
        public IEnumerable<string> GetCompletions()
        {
            return _completions;
        }

        public Shell(IConsole console, FileSystem fs)
        {
            _console = console;
            _console.AutoCompleteSource = this;
            _fs = fs;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            RegisterBuiltin("clear", "Clear the screen", _console.Clear);
            RegisterBuiltin("echo", "Write text to the screen", Echo);
            RegisterBuiltin("ls", "List the current working directory.", Ls);
            RegisterBuiltin("cd", "Change directory", ChangeWorkingDirectory);
            RegisterBuiltin("cat", "Show a file's contents", Cat);

            UpdateCompletions();
            
            WritePrompt();
        }

        private void Cat(IConsole console, string name, string[] args)
        {
            if (args.Length < 1)
                throw new SyntaxErrorException($"{name}: usage: {name} <path>");

            var path = ResolvePath(args.First());

            try
            {
                var text = _fs.ReadAllText(path);
                console.WriteLine(text);
            }
            catch (Exception ex)
            {
                console.WriteLine("{0}: {1}: {2}", name, path, ex.Message);
            }
        }

        private string ResolvePath(string path)
        {
            if (!path.StartsWith(PathUtils.Separator))
            {
                path = PathUtils.Combine(_work, path);
            }

            var resolved = PathUtils.Resolve(path);
            return resolved;
        }
        
        private void ChangeWorkingDirectory(IConsole console, string name, string[] args)
        {
            if (args.Length < 1)
                throw new SyntaxErrorException($"{name}: usage: {name} <path>");

            var path = args.First();

            var resolved = ResolvePath(path);

            if (_fs.DirectoryExists(resolved))
            {
                _work = resolved;
                UpdateCompletions();
            }
            else
            {
                throw new SyntaxErrorException($"{name}: {path}: Directory not found.");
            }
        }
        
        private void Ls(IConsole console, string name, string[] args)
        {
            foreach (var dir in _fs.GetDirectories(_work))
            {
                console.WriteLine(dir);
            }

            foreach (var file in _fs.GetFiles(_work))
            {
                console.WriteLine(file);
            }
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