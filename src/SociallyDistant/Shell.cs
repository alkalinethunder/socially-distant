using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MimeTypes.Core;
using SociallyDistant.Core.Commands;
using SociallyDistant.Core.Components;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.Displays;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.IO;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Social;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Ecs;
using Thundershock.Gui.Elements.Console;
using Thundershock.IO;

namespace SociallyDistant.Core
{
    public class Shell : IAutoCompleteSource, ISystem
    {
        private Scene _scene;
        private List<string> _completions = new List<string>();
        private List<CommandInfo> _commands = new List<CommandInfo>();
        private List<Builtin> _builtins = new List<Builtin>();
        private Dictionary<uint, Queue<Instruction>> _instructions = new();
        private Dictionary<string, Type> _displayMap = new();

        public event EventHandler<DisplayRequestedEventArgs> DisplayRequested;
        
        public void RegisterBuiltin(string name, string desc, Action<IRedTeamContext, IConsole, string, string[]> action)
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

        public void RegisterBuiltin(string name, string desc, Action<IConsole, string, string[]> action)
        {
            RegisterBuiltin(name, desc, (_, console, name, args) => action(console, name, args));
        }

        public void RegisterBuiltin(string name, string desc, Action<IConsole> action)
        {
            RegisterBuiltin(name, desc, (console, _, _) => action(console));
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
        }
        
        private void WritePrompt(IConsole console, ShellStateComponent state, User user, string host)
        {
            // this will force the console to reset all colors and attributes to their defaults.
            console.Write("&0");

            if (user != null && !string.IsNullOrWhiteSpace(user.UserName) && !string.IsNullOrWhiteSpace(host))
            {
                var home = string.IsNullOrWhiteSpace(user.HomeDirectory) ? "/" : user.HomeDirectory;

                var shebang = user.Type == UserType.Root ? "#" : "$";
                
                var work = state.WorkingDirectory;
                if (work.StartsWith(home))
                    work = PathUtils.Home + work.Substring(home.Length);
                console.Write("{0}@{1}:{2}{3} ", user.UserName, host, work, shebang);
            }
            else
            {
                console.Write("vOS shell (no device) >>> ");
            }
        }
        
        public void RegisterBuiltin(string name, string desc, Action action)
        {
            RegisterBuiltin(name, desc, (_, _, _) => action());
        }
        
        public IEnumerable<string> GetCompletions(string word)
        {
            return Enumerable.Empty<string>();
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
        
        private void BootUp()
        {
            RegisterBuiltin("commands", "Show a list of built-in commands", PrintBuiltins);
            RegisterBuiltin("programs", "Show a list of installed programs.", PrintExternals);
            RegisterBuiltin("help", "Show the full help text.", PrintHelp);
            RegisterBuiltin("clear", "Clear the screen", (console, _, _) => console.Clear());
            RegisterBuiltin("echo", "Write text to the screen", Echo);
            RegisterBuiltin("cd", "Change directory", ChangeWorkingDirectory);
            
            foreach (var type in ThundershockPlatform.GetAllTypes<Command>())
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
        }
        
        private string ResolvePath(string work, string home, string path)
        {
            if (path.StartsWith(PathUtils.Home))
            {
                path = PathUtils.Combine(home, path.Substring(PathUtils.Home.Length));
            }
            
            if (!path.StartsWith(PathUtils.Separator))
            {
                path = PathUtils.Combine(work, path);
            }

            var resolved = PathUtils.Resolve(path);
            return resolved;
        }
        
        private void ChangeWorkingDirectory(IRedTeamContext ctx, IConsole console, string name, string[] args)
        {
            if (args.Length < 1)
                throw new SyntaxErrorException($"{name}: usage: {name} <path>");

            var path = args.First();

            var resolved = ResolvePath(ctx.WorkingDirectory, ctx.HomeDirectory, path);

            if (ctx.Vfs.DirectoryExists(resolved))
            {
                ctx.SetWorkingDirectory(resolved);
                UpdateCompletions();
            }
            else
            {
                throw new SyntaxErrorException($"{name}: {path}: Directory not found.");
            }
        }

        private bool TryOpenGui(string filePath)
        {
            // FIXME
            return false;
        }

        public void Init(Scene scene)
        {
            _scene = scene;
            
            // BANG, this is the power of Thundershock, we ONLY have to do this once.
            this.BootUp();
        }

        public void Unload()
        {
        }

        public void Load()
        {
            
        }

        public IRedTeamContext CreatePlayerContext()
        {
            var shells = _scene.Registry.View<IConsole, ShellStateComponent, PlayerState>();

            var entity = shells.First();
            
            var state = _scene.Registry.GetComponent<ShellStateComponent>(entity);
            var fs = _scene.Registry.GetComponent<FileSystem>(entity);
            var device = _scene.Registry.GetComponent<DeviceData>(entity);
            var mailbox = _scene.Registry.GetComponent<Mailbox>(entity);
            var user = device != null ? device.GetUser(state.UserId) : null;

            return new UserContext(this, state, device, user, fs, mailbox);
        }
        
        public void Update(GameTime gameTime)
        {
            // Loop through all entities containing a console and a shell state.
            foreach (var entity in _scene.Registry.View<IConsole, ShellStateComponent>())
            {
                var console = _scene.Registry.GetComponent<IConsole>(entity);
                var state = _scene.Registry.GetComponent<ShellStateComponent>(entity);
                var fs = _scene.Registry.GetComponent<FileSystem>(entity);
                var device = _scene.Registry.GetComponent<DeviceData>(entity);

                state.FrameTime = gameTime.ElapsedGameTime;

                state.UpTime += state.FrameTime;

                state.TerminalName = "/dev/pts" + entity;
                state.ShellName = "bash 5.1.8";

                UpdateEntity(gameTime, entity, state, console, device, fs);

            }
        }

        private void UpdateEntity(GameTime gameTime, uint entity, ShellStateComponent state, IConsole console, DeviceData device, FileSystem fs)
        {
            // Create the command queue for this entity if it doesn't already exist.
            if (!_instructions.ContainsKey(entity))
                _instructions.Add(entity, new Queue<Instruction>());

            var mailbox = _scene.Registry.GetComponent<Mailbox>(entity);
            var host = (device != null) ? device.HostName : null;
            var user = device != null ? device.GetUser(state.UserId) : null;

            if (string.IsNullOrEmpty(state.WorkingDirectory))
            {
                if (!string.IsNullOrWhiteSpace(user.HomeDirectory))
                {
                    state.WorkingDirectory = user.HomeDirectory;
                }
                else
                {
                    state.WorkingDirectory = "/";
                }
            }
            
            if (state.IsExecuting)
            {
                var activeExternal = _scene.Registry.GetComponent<Command>(entity);
                
                if (activeExternal != null)
                {
                    activeExternal.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
                    if (activeExternal.IsCompleted)
                    {
                        _scene.Registry.RemoveComponent<Command>(entity);
                    }
                    else  return;
                }
                
                while (_instructions[entity].Any())
                {
                    var ins = _instructions[entity].Dequeue();

                    if (!TryOpenGui(ins.Name))
                    {
                        var uc = new UserContext(this, state, device, user, fs, mailbox);

                        if (!ProcessBuiltin(uc, ins.Console, ins.Name, ins.Args))
                        {
                            var cmd = FindCommand(ins.Name);
                            if (cmd == null)
                            {
                                ins.Console.WriteLine("{0}: {1}: Command not found.", "sh", ins.Name);
                            }
                            else
                            {
                                _scene.Registry.AddComponent(entity, cmd);
                                cmd.Run(ins.Args, state.WorkingDirectory, ins.Console, uc);
                                return;
                            }
                        }
                    }

                    if (ins.Console is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                
                // reset console formatting
                WritePrompt(console, state, user, host);
                state.IsExecuting = false;
            }
            else
            {
                if (console.GetLine(out string line))
                {
                    console.Write("&0");
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        ProcessCommand(entity, console, state, line, user, host, fs);
                    }
                    else
                    {
                        WritePrompt(console, state, user, host);
                    }
                }
            }

        }

        public void Render(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        private void Echo(IConsole console, string name, string[] args)
        {
            console.WriteLine(string.Join(" ", args));
        }
        
        private bool ProcessBuiltin(IRedTeamContext ctx, IConsole console, string name, string[] args)
        {
            var builtin = _builtins.FirstOrDefault(x => x.Name == name);
            if (builtin != null)
            {
                if (builtin.Action != null)
                {
                    try
                    {
                        builtin.Action(ctx, console, name, args);
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

        private IEnumerable<Instruction> ProcessTokens(IConsole console, string[] words, string work, string home, FileSystem fs)
        {
            var pipe = new Pipe(console, console);

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
                    file = ResolvePath(work, home, file);

                    try
                    {
                        var fileConsole = fs.CreateFileConsole(pipe, file, word == ">>");
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

                    var buffer = new BufferConsole();
                    pipe.Output = buffer;

                    ins.Console = pipe;
                    yield return ins;

                    pipe = new Pipe(buffer, console);
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
        
        private void ProcessCommand(uint entity, IConsole console, ShellStateComponent state, string commandLine, User user, string host, FileSystem fs)
        {
            try
            {
                var words = CommandShellUtils.BreakLine(commandLine);

                if (words.Any())
                {
                    foreach (var ins in ProcessTokens(console, words, state.WorkingDirectory, user.HomeDirectory, fs))
                        _instructions[entity].Enqueue(ins);

                    state.IsExecuting = true;
                    console.Write("&0");
                }
                else
                {
                    WritePrompt(console, state, user, host);
                }
            }
            catch (SyntaxErrorException ex)
            {
                console.WriteLine("sh: error: {0}", ex.Message);
                WritePrompt(console, state, user, host);
            }
        }
        
        private class Builtin
        {
            public string Name;
            public string Description;
            public Action<IRedTeamContext, IConsole, string, string[]> Action;
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

    public sealed class DisplayRequestedEventArgs : EventArgs
    {
        public IRedTeamContext Context { get; }
        public string FilePath { get; }
        public DisplayWindow DisplayWindow { get; }

        public DisplayRequestedEventArgs(IRedTeamContext ctx, string file)
        {
            Context = ctx;
            FilePath = file;
            DisplayWindow = DisplayWindow;
        }
    }

    public sealed class UserContext : IRedTeamContext
    {
        private Shell _shell;
        private ShellStateComponent _shellState;
        private DeviceData _device;
        private User _user;
        private FileSystem _fs;
        private Mailbox _mailbox;

        public Mailbox Mailbox => _mailbox;
        public void SetWorkingDirectory(string work)
        {
            _shellState.WorkingDirectory = work;
        }
        
        public string WorkingDirectory => _shellState.WorkingDirectory;
        public int ScreenWidth => _shellState.ScreenWidth;
        public int ScreenHeight => _shellState.ScreenHeight;
        public TimeSpan FrameTime => _shellState.FrameTime;
        public TimeSpan Uptime => _shellState.UpTime;
        public bool IsGraphical => _shellState.IsGraphical;
        public FileSystem Vfs => _fs;
        public string UserName => _user.UserName;
        public string HostName => _device.HostName;
        public string HomeDirectory => _user.HomeDirectory;
        public string Terminal => _shellState.TerminalName;
        public string Shell => _shellState.ShellName;
        public string WindowManager => _shellState.WindowManagerName;
        public string DesktopEnvironment => _shellState.DesktopName;
        
        public void ShutDown()
        {
            
        }

        public UserContext(Shell shell, ShellStateComponent shellState, DeviceData device, User user, FileSystem fs, Mailbox mailbox)
        {
            _shell = shell;
            _shellState = shellState;
            _user = user;
            _device = device;
            _fs = fs;
            _mailbox = mailbox;
        }
    }
}