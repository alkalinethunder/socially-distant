using System.Collections.Generic;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using RedTeam.Commands;
using RedTeam.IO;

namespace RedTeam
{
    public class Shell : SceneComponent, IAutoCompleteSource
    {
        private SoundEffect _cmdNotFound;
        
        private string _home;
        private List<string> _completions = new List<string>();
        private IConsole _console;
        private List<CommandInfo> _commands = new List<CommandInfo>();
        private List<Builtin> _builtins = new List<Builtin>();
        private FileSystem _fs;
        private string _work = "/";
        private bool _executing;
        private Queue<Instruction> _instructions = new Queue<Instruction>();
        private Command _activeExternal;
        private IRedTeamContext _userContext;
        
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

        public void RegisterBuiltin(string name, string desc, Action<IConsole> action)
        {
            RegisterBuiltin(name, desc, (console, name, args) => action(console));
        }

        private void UpdateCompletions()
        {
            _completions.Clear();
            
            // built-ins
            foreach (var builtin in _builtins)
                _completions.Add(builtin.Name);
            
            // external programs
            foreach (var cmd in _commands)
                _completions.Add(cmd.Name);
            
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
            // this will force the console to reset all colors and attributes to their defaults.
            _console.Write("&0");

            var work = _work;
            if (work.StartsWith(_home))
                work = PathUtils.Home + work.Substring(_home.Length);
            _console.Write("{0}@{1}:{2}# ", _userContext.UserName, _userContext.HostName, work);
        }
        
        public void RegisterBuiltin(string name, string desc, Action action)
        {
            RegisterBuiltin(name, desc, (console, cmd, args) => action());
        }
        
        public IEnumerable<string> GetCompletions(string word)
        {
            foreach (var predefined in _completions)
                yield return predefined;

            var path = ResolvePath(word);
            var dir = path;
            if (!word.EndsWith(PathUtils.Separator))
                dir = PathUtils.GetDirectoryName(dir);

            if (_fs.DirectoryExists(dir))
            {
                foreach (var name in _fs.GetDirectories(dir))
                {
                    var escaped = name.Replace(" ", "\\ ");

                    if (name.ToLower().StartsWith(word.ToLower()) || escaped.ToLower().StartsWith(word.ToLower()))
                    {
                        yield return escaped + PathUtils.Separator;
                        yield return $"\"{name}{PathUtils.Separator}\"";
                    }
                }
                
                foreach (var name in _fs.GetFiles(dir))
                {
                    var escaped = name.Replace(" ", "\\ ");

                    if (name.ToLower().StartsWith(word.ToLower()) || escaped.ToLower().StartsWith(word.ToLower()))
                    {
                        yield return escaped;
                        yield return $"\"{name}\"";
                    }
                }
            }
        }

        public Shell(IConsole console, FileSystem fs, IRedTeamContext ctx)
        {
            _console = console;
            _console.AutoCompleteSource = this;
            _fs = fs;
            _userContext = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _home = _userContext.HomeDirectory;
        }

        private void PrintBuiltins(IConsole console)
        {
            console.WriteLine("Commands:");
            console.WriteLine();

            var longest = _builtins.Select(x => x.Name).OrderByDescending(x => x.Length).First().Length + 2;
            
            foreach (var cmd in _builtins.OrderBy(x=>x.Name))
            {
                console.Write(" - #d{0}&0", cmd.Name);
                if (!string.IsNullOrWhiteSpace(cmd.Description))
                {
                    console.Write(":");
                    for (var i = 0; i < longest - cmd.Name.Length; i++)
                        console.Write(" ");
                    console.Write("&w{0}&W", cmd.Description);
                }

                console.WriteLine();
            }
        }
        
        private void PrintExternals(IConsole console)
        {
            console.WriteLine("Programs:");
            console.WriteLine();

            var longest = _commands.Select(x => x.Name).OrderByDescending(x => x.Length).First().Length + 2;
            
            foreach (var cmd in _commands.OrderBy(x=>x.Name))
            {
                console.Write(" - #9{0}&0", cmd.Name);
                if (!string.IsNullOrWhiteSpace(cmd.Description))
                {
                    console.Write(":");
                    for (var i = 0; i < longest - cmd.Name.Length; i++)
                        console.Write(" ");
                    console.Write("&w{0}&W", cmd.Description);
                }

                console.WriteLine();
            }
        }

        public void PrintHelp(IConsole console)
        {
            console.WriteLine("COMMAND HELP");
            console.WriteLine("============");
            console.WriteLine();
            PrintBuiltins(console);
            console.WriteLine();
            PrintExternals(console);
        }
        
        protected override void OnLoad()
        {
            base.OnLoad();

            _cmdNotFound = Game.Content.Load<SoundEffect>("Sounds/Redsh/cmdNotFound");
            
            if (_fs.DirectoryExists(_home))
            {
                _work = _home;
            }
            else
            {
                _console.WriteLine(
                    "sh: warning: user home directory was not found on disk, falling back to root directory /");
            }
            
            RegisterBuiltin("commands", "Show a list of built-in commands", PrintBuiltins);
            RegisterBuiltin("programs", "Show a list of installed programs.", PrintExternals);
            RegisterBuiltin("help", "Show the full help text.", PrintHelp);
            RegisterBuiltin("clear", "Clear the screen", (console, name, args) => console.Clear());
            RegisterBuiltin("echo", "Write text to the screen", Echo);
            RegisterBuiltin("cd", "Change directory", ChangeWorkingDirectory);

            foreach (var type in RedTeamPlatform.GetAllTypes<Command>())
            {
                var cmd = (Command) Activator.CreateInstance(type, null);

                if (_commands.Any(x => x.Name == cmd.Name))
                    throw new InvalidOperationException($"Command {cmd.Name} already taken by another command.");

                var info = new CommandInfo();
                info.Name = cmd.Name;
                info.Description = cmd.Description;
                info.Type = type;

                _commands.Add(info);
            }
            
            UpdateCompletions();
            
            WritePrompt();
        }
        
        private string ResolvePath(string path)
        {
            if (path.StartsWith(PathUtils.Home))
            {
                path = PathUtils.Combine(_home, path.Substring(PathUtils.Home.Length));
            }
            
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
        
        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (_executing)
            {
                if (_activeExternal != null)
                {
                    _activeExternal.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
                    if (_activeExternal.IsCompleted)
                    {
                        _activeExternal = null;
                    }
                    else  return;
                }
                
                while (_instructions.Any())
                {
                    var ins = _instructions.Dequeue();

                    if (!ProcessBuiltin(ins.Console, ins.Name, ins.Args))
                    {
                        var cmd = FindCommand(ins.Name);
                        if (cmd == null)
                        {
                            ins.Console.WriteLine("{0}: {1}: Command not found.", "sh", ins.Name);
                            _cmdNotFound.Play();
                        }
                        else
                        {
                            _activeExternal = cmd;
                            _activeExternal.Run(ins.Args, _work, _fs, ins.Console, _userContext);
                            return;
                        }
                    }

                    if (ins.Console is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                
                // reset console formatting
                _console.Write("&0");
                WritePrompt();
                _executing = false;
            }
            else
            {
                if (_console.GetLine(out string line))
                {
                    _console.Write("&0");
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
        }

        private void Echo(IConsole console, string name, string[] args)
        {
            console.Write(string.Join(" ", args));
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

        private bool ProcessBuiltin(IConsole console, string name, string[] args)
        {
            var builtin = _builtins.FirstOrDefault(x => x.Name == name);
            if (builtin != null)
            {
                if (builtin.Action != null)
                {
                    try
                    {
                        builtin.Action(console, name, args);
                    }
                    catch (SyntaxErrorException sex)
                    {
                        // reset console formatting
                        console.Write("&0");
                        console.WriteLine("sh: {0}", sex.Message);
                    }
                    catch (Exception ex)
                    {
                        // reset console formatting
                        console.Write("&0");

                        console.WriteLine(
                            "#csh: &b&2&u/!\\&U FATAL GAMEDEV FUCKUP DETECTED IN REDSH BUILT-IN &u/!\\&U&0");
                        console.WriteLine(ex.ToString());
                    }
                    finally
                    {
                        console.WriteLine();
                    }
                    
                    return true;
                }
            }

            return false;
        }

        private Command FindCommand(string name)
        {
            var info = _commands.FirstOrDefault(x => x.Name == name);
            if (info != null)
            {
                return (Command) Activator.CreateInstance(info.Type, null);
            }

            return null;
        }

        private IEnumerable<Instruction> ProcessTokens(string[] words)
        {
            var pipe = new Pipe(_console, _console);

            var ins = null as Instruction;
            
            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];

                if (word == ">" || word == ">>")
                {
                    if (ins == null)
                        throw new SyntaxErrorException("expected command before " + word);
                    
                    ins.CheckName();

                    var file = string.Join(" ", words.Skip(i + 1));
                    file = ResolvePath(file);

                    try
                    {
                        var fileConsole = _fs.CreateFileConsole(pipe, file, word == ">>");
                        pipe.Output = fileConsole;
                    }
                    catch (Exception ex)
                    {
                        throw new SyntaxErrorException($"couldn't open {file}: {ex.Message}");
                    }
                    
                    break;
                }
                
                if (word == "|")
                {
                    if (ins == null)
                        throw new SyntaxErrorException("expected command before " + word);
                    
                    ins.CheckName();

                    var output = pipe.Output;
                    var buffer = new BufferConsole();
                    pipe.Output = buffer;

                    ins.Console = pipe;
                    yield return ins;

                    pipe = new Pipe(buffer, _console);
                    ins = null;
                    continue;
                }

                if (ins == null)
                {
                    ins = new Instruction();
                    ins.Name = word;
                    continue;
                }

                ins.AddArgument(word);
            }

            if (ins != null)
            {
                ins.Console = pipe;
                yield return ins;
            }
        }
        
        private void ProcessCommand(string commandLine)
        {
            try
            {
                var words = BreakLine(commandLine);

                if (words.Any())
                {
                    foreach (var ins in ProcessTokens(words))
                        _instructions.Enqueue(ins);

                    _executing = true;
                    _console.Write("&0");
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

        private class CommandInfo
        {
            public string Name;
            public string Description;

            public Type Type;
        }
        
        private class Instruction
        {
            public string Name;
            public string[] Args = Array.Empty<string>();
            public IConsole Console;

            public void AddArgument(string arg)
            {
                Array.Resize(ref Args, Args.Length + 1);
                Args[^1] = arg;
            }
            
            public void CheckName()
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    throw new SyntaxErrorException("command expected");
                }
            }
        }
    }
}