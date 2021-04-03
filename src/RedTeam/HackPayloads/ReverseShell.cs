using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.VisualBasic.FileIO;
using RedTeam.Commands;

namespace RedTeam.HackPayloads
{
    public class ReverseShell : IPayload
    {
        private List<Builtin> _builtins = new();
        private List<CommandInfo> _commands = new();
        private IRedTeamContext _ctx;
        private IConsole _console;
        private string _work = "/";
        private bool _executing;
        private Command _command;
        
        public void Init(IConsole console, IRedTeamContext ctx)
        {
            _console = console;
            _ctx = ctx;

            RegisterBuiltin("exit", "Disconnect.", Exit);
            
            RegisterCommand<Nmap>();
            RegisterCommand<Cat>();
            RegisterCommand<ListDirectory>();
            RegisterCommand<WhoAmI>();

            WritePrompt();
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
    }
}