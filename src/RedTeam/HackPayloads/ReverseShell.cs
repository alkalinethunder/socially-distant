using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using RedTeam.Commands;
using Thundershock.IO;

namespace RedTeam.HackPayloads
{
    public class ReverseShell : IPayload
    {
        private IRedTeamContext _player;
        private List<Builtin> _builtins = new();
        private List<CommandInfo> _commands = new();
        private IRedTeamContext _ctx;
        private IConsole _console;
        private string _work = "/";
        private bool _executing;
        private Command _command;
        
        public void Init(IConsole console, IRedTeamContext ctx, IRedTeamContext player)
        {
            _player = player;
            _console = console;
            _ctx = ctx;

            RegisterBuiltin("exit", "Disconnect.", Exit);
            RegisterBuiltin("help", "Show the help text.", PrintHelp);
            RegisterBuiltin("clear", "Clear the screen.", _console.Clear);
            RegisterBuiltin("cd", "Change working directory", ChangeWorkingDirectory);

            RegisterCommand<PrintWorkingDirectory>();
            RegisterCommand<Nmap>();
            RegisterCommand<Cat>();
            RegisterCommand<ListDirectory>();
            RegisterCommand<WhoAmI>();
            RegisterCommand<IfConfig>();
            RegisterCommand<Ping>();
            RegisterCommand<WriteCommand>();
            
            WritePrompt();
        }

        private string ResolvePath(string path)
        {
            if (path.StartsWith(PathUtils.Home))
            {
                path = PathUtils.Combine(_ctx.HomeDirectory, path.Substring(PathUtils.Home.Length));
            }
            
            if (!path.StartsWith(PathUtils.Separator))
            {
                path = PathUtils.Combine(_work, path);
            }

            var resolved = PathUtils.Resolve(path);
            return resolved;
        }
        
        private void ChangeWorkingDirectory(string name, string[] args)
        {
            if (args.Length < 1)
            {
                _console.WriteLine($"{name}: usage: {name} <path>");
                return;
            }

            var path = args.First();

            var resolved = ResolvePath(path);

            if (_ctx.Vfs.DirectoryExists(resolved))
            {
                _work = resolved;
            }
            else
            {
                _console.WriteLine($"{name}: {path}: Directory not found.");
            }
        }

        
        private void RegisterCommand<T>() where T : Command, new()
        {
            var cmd = new T();

            var info = new CommandInfo();
            info.Name = cmd.Name;
            info.Description = cmd.Description;
            info.Command = typeof(T);

            _commands.Add(info);
        }

        private void RegisterBuiltin(string name, string desc, Action<string[]> action)
            => RegisterBuiltin(name, desc, (n, a) => action(a));

        private void RegisterBuiltin(string name, string desc, Action action)
            => RegisterBuiltin(name, desc, (a) => action());

        private void RegisterBuiltin(string name, string desc, Action<string, string[]> runner)
        {
            var b = new Builtin();
            b.Name = name;
            b.Description = desc;
            b.Action = runner;
            
            _builtins.Add(b);
        }
        
        private void WritePrompt()
        {
            _console.Write("#cred#d&brev&0 {0}:#b{1} #f&b&2>>>&0 ", _ctx.HostName, _work);
        }

        private void Exit()
        {
            _ctx.ShutDown();
        }
        
        private bool ExecBuiltin(string name, string[] args)
        {
            var b = _builtins.FirstOrDefault(x => x.Name == name);

            if (b != null)
            {
                b.Action(name, args);
                return true;
            }

            return false;
        }

        private bool ExecShellCommand(string name, string[] args)
        {
            var cmd = _commands.FirstOrDefault(x => x.Name == name);

            if (cmd != null)
            {
                var type = cmd.Command;
                var cmdInstance = (Command) Activator.CreateInstance(type, null);

                _executing = true;
                _command = cmdInstance;

                _command.Run(args, _work, _ctx.Vfs, _console, _ctx, false);
                
                return true;
            }

            return false;
        }
        
        private void ProcessCommand(string text)
        {
            try
            {
                var tokens = Thundershock.CommandShellUtils.BreakLine(text);

                var name = tokens.First();

                var args = tokens.Skip(1).ToArray();

                if (!ExecShellCommand(name, args))
                {
                    if (!ExecBuiltin(name, args))
                    {
                        _console.WriteLine("Command '{0}' unknown.", name);
                        WritePrompt();
                    }
                }
            }
            catch (SyntaxErrorException ex)
            {
                _console.WriteLine(ex.Message);
                WritePrompt();
            }
        }
        
        public void Update(float deltaTime)
        {
            if (_executing)
            {
                if (_command != null)
                {
                    _command.Update(deltaTime);
                    if (_command.IsCompleted)
                    {
                        _command = null;
                    }
                }
                else
                {
                    _executing = false;
                    WritePrompt();
                }
            }
            else
            {
                if (_console.GetLine(out string text))
                {
                    var trimmed = text.Trim();

                    if (string.IsNullOrWhiteSpace(trimmed))
                    {
                        WritePrompt();
                    }
                    else
                    {
                        ProcessCommand(text);
                    }
                }
            }
        }

        private class CommandInfo
        {
            public string Name;
            public string Description;
            public Type Command;
        }

        private class Builtin
        {
            public string Name;
            public string Description;
            public Action<string, string[]> Action;
        }
        
        private void PrintBuiltins()
        {
            _console.WriteLine("Commands:");
            _console.WriteLine();

            var longest = _builtins.Select(x => x.Name).OrderByDescending(x => x.Length).First().Length + 2;
            
            foreach (var cmd in _builtins.OrderBy(x=>x.Name))
            {
                _console.Write(" - #d{0}&0", cmd.Name);
                if (!string.IsNullOrWhiteSpace(cmd.Description))
                {
                    _console.Write(":");
                    for (var i = 0; i < longest - cmd.Name.Length; i++)
                        _console.Write(" ");
                    _console.Write("&w{0}&W", cmd.Description);
                }

                _console.WriteLine();
            }
        }
        
        private void PrintExternals()
        {
            _console.WriteLine("Programs:");
            _console.WriteLine();

            var longest = _commands.Select(x => x.Name).OrderByDescending(x => x.Length).First().Length + 2;
            
            foreach (var cmd in _commands.OrderBy(x=>x.Name))
            {
                _console.Write(" - #9{0}&0", cmd.Name);
                if (!string.IsNullOrWhiteSpace(cmd.Description))
                {
                    _console.Write(":");
                    for (var i = 0; i < longest - cmd.Name.Length; i++)
                        _console.Write(" ");
                    _console.Write("&w{0}&W", cmd.Description);
                }

                _console.WriteLine();
            }
        }

        public void PrintHelp()
        {
            _console.WriteLine("COMMAND HELP");
            _console.WriteLine("============");
            _console.WriteLine();
            PrintBuiltins();
            _console.WriteLine();
            PrintExternals();
        }
    }
}