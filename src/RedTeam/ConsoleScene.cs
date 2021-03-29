using System;
using Microsoft.Xna.Framework;
using Thundershock.Gui;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RedTeam.Config;
using RedTeam.IO;
using RedTeam.SaveData;
using Thundershock;

namespace RedTeam
{
    public class ConsoleScene : Scene
    {
        private string _name;
        private string _username;
        private Queue<string> _queue = new Queue<string>();
        private bool _isBooted = false;
        private int _bootPhase;
        private double _timer;
        private GuiSystem _guiSystem;
        private ConsoleControl _console;
        private Shell _shell;
        private SaveManager _saveManager;
        private RedConfigManager _redConfig;
        
        protected override void OnLoad()
        {
            _redConfig = App.GetComponent<RedConfigManager>();
            _saveManager = App.GetComponent<SaveManager>();
            
            _guiSystem = AddComponent<GuiSystem>();
            _console = new ConsoleControl();
            _guiSystem.AddToViewport(_console);

            _console.WriteLine(" * checking for redteam os container *");

            if (_saveManager.IsSaveAvailable)
            {
                _saveManager.LoadGame();
                StartShell();
            }
            else
            {
                _saveManager.NewGame();
                _console.WriteLine(" * container image not found *");
                _timer = 0.5;
            }

            _console.ColorPalette = _redConfig.GetPalette();
            
            _redConfig.ConfigUpdated += ApplyConfig;
        }

        private void ApplyConfig(object? sender, EventArgs e)
        {
            _console.ColorPalette = _redConfig.GetPalette();
        }

        private void StartShell()
        {
            _isBooted = true;

            var ctx = _saveManager.CreatePlayerContexxt();
            var fs = ctx.Vfs;
            _shell = new Shell(_console, fs, ctx);
            AddComponent(_shell);
        }
        
        private void PrintIntro()
        {
            var resource = this.GetType().Assembly.GetManifestResourceStream("RedTeam.Resources.Intro.txt");
            
            using var reader = new StreamReader(resource, Encoding.UTF8, true);

            var text = reader.ReadToEnd();

            var lines = text.Split(Environment.NewLine);

            foreach (var line in lines)
                _queue.Enqueue(line);

            _queue.Enqueue("");
            _timer = 0.1;
        }

        private void PrintAgentPrompt()
        {
            _console.WriteLine(" * creating new redteam-os container *");
            _console.WriteLine();
            _console.WriteLine(" * setup 1/2: agent information *");
            _console.WriteLine();
            _console.Write("What is your name? >>> ");
        }

        private bool ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 2)
                return false;

            var first = username[0];

            if (username != username.ToLower())
                return false;
            
            if (!char.IsLetter(first))
                return false;
            
            foreach (var ch in username)
                if (!(char.IsLetterOrDigit(ch) || ch == '_'))
                    return false;

            return true;
        }

        private bool ValidateName(string name)
        {
            var res = false;
            
            foreach (var ch in name)
                if (char.IsLetter(ch))
                    res = true;
            
            return res && !string.IsNullOrWhiteSpace(name) && name.Length >= 3;
        }

        private string Unixify(string name)
        {
            var trimmed = name.Trim();
            var lower = trimmed.ToLower();

            while (!char.IsLetter(lower[0]))
            {
                var ch = lower[0];
                lower = lower.Substring(1);
                if (!char.IsWhiteSpace(ch))
                    lower += ch;
            }

            for (var i = 0; i < lower.Length; i++)
            {
                var ch = lower[i];
                if (!char.IsLetterOrDigit(ch) && ch != '_')
                {
                    lower = lower.Replace(ch, '_');
                }
            }

            return lower;
        }

        private void PrintUnixConfirm()
        {
            _username = Unixify(_name);

            _console.WriteLine();
            _console.WriteLine("Your name is &b{0}&B. Your auto-generated UNIX username is &b{1}&B.", _name, _username);
            _console.WriteLine();
            _console.Write("Change username? [y/N] ");
        }

        private void CreateContainer()
        {
            var dev = _saveManager.CreateDevice(_username + "-pc");
            var id = _saveManager.CreateIdentity(_username, _name);

            _saveManager.SetPlayerInfo(id, dev);
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            if (!_isBooted)
            {
                switch (_bootPhase)
                {
                    case 0:
                        if (_timer <= 0)
                        {
                            PrintIntro();
                            _bootPhase++;
                        }
                        else
                        {
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        break;
                    case 1:
                        if (_timer <= 0)
                        {
                            if (_queue.TryDequeue(out string line))
                            {
                                _console.WriteLine(line);
                                _timer = 0.1;
                            }
                            else
                            {
                                PrintAgentPrompt();
                                _bootPhase++;
                            }
                        }
                        else
                        {
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        break;
                    case 2:
                        if (_console.GetLine(out string username))
                        {
                            username = username.Trim();
                            if (ValidateName(username))
                            {
                                _name = username;
                                _bootPhase++;
                                PrintUnixConfirm();
                            }
                            else
                            {
                                _console.WriteLine("#cERROR:&0 Valid name of at least 3 characters required.");
                                _console.WriteLine();
                                PrintAgentPrompt();
                            }
                        }
                        break;
                    case 3:
                        if (_console.GetLine(out string ans))
                        {
                            var trim = ans.Trim().ToLower();
                            if (trim == "y")
                            {
                                _bootPhase = 4;
                                _console.Write("Enter new UNIX username: ");
                            }
                            else if (trim == "n")
                            {
                                _bootPhase = 5;
                            }
                            else
                            {
                                _console.Write("Change username? [y/N] ");
                            }
                        }
                        break;
                    case 4:
                        if (_console.GetLine(out string uname))
                        {
                            if (!ValidateUsername(uname))
                            {
                                _console.WriteLine(
                                    " * must be all lower-case, at least 2 characters in length, start with a letter, and only contain alphanumeric text and underscores.");
                                _console.Write("Enter new UNIX username: ");
                            }
                            else
                            {
                                _username = uname;
                                _bootPhase++;
                            }
                        }
                        break;
                    case 5:
                        if (_timer > 0)
                        {
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            _console.WriteLine(" * installing redsh *");
                            _timer = 1;
                            _bootPhase++;
                        }
                        break;
                    case 6:
                        if (_timer > 0)
                        {
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;   
                        }
                        else
                        {
                            _console.WriteLine(" * writing /etc/hostname *");
                            _console.WriteLine("echo {0}-pc > /etc/hostname", _username);
                            _console.WriteLine("mkdir /home/{0}", _username);
                            _timer = 0.5;
                            _bootPhase++;
                        }
                        break;
                    case 7:
                        if (_timer > 0)
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;
                        else
                        {
                            _console.WriteLine(" * container ready *");
                            _timer = 0.2;
                            _bootPhase++;
                        }
                        break;
                    case 8:
                        if (_timer > 0)
                            _timer -= gameTime.ElapsedGameTime.TotalSeconds;
                        else
                        {
                            _console.WriteLine(" * connected *");
                            CreateContainer();
                            StartShell();
                        }
                        break;
                }
            }
        }
    }
}