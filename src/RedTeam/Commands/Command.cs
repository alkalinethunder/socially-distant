using System;
using RedTeam.Net;
using Thundershock;
using Thundershock.Debugging;
using Thundershock.IO;

namespace RedTeam.Commands
{
    public abstract class Command
    {
        private NetworkManager _network;
        private string _home;
        private IRedTeamContext _userContext;
        private bool _completed = false;
        private bool _running = false;
        private IConsole _console;
        private FileSystem _fs;
        private string[] _args;
        private string _workingDirectory;
        private bool _disposeConsole;
        
        public abstract string Name { get; }
        public virtual string Description => string.Empty;

        protected NetworkManager Network => _network;
        protected IRedTeamContext Context => _userContext;
        protected IConsole Console => _console;
        protected string[] Arguments => _args;
        protected string WorkingDirectory => _workingDirectory;
        protected FileSystem FileSystem => _fs;
        
        public bool IsCompleted => _completed;

        protected string ResolvePath(string path)
        {
            var resolved = path;
            if (resolved.StartsWith(PathUtils.Home))
                return PathUtils.Resolve(PathUtils.Combine(_home, resolved.Substring(PathUtils.Home.Length)));
            if (!resolved.StartsWith(PathUtils.Separator))
                return PathUtils.Resolve(_workingDirectory, path);
            return PathUtils.Resolve(resolved);
        }
        
        public void Run(string[] args, string work, FileSystem fs, IConsole console, IRedTeamContext ctx, bool disposeConsole = true)
        {
            if (_running)
                throw new InvalidOperationException("Command has already been run.");

            _disposeConsole = disposeConsole;
            _userContext = ctx ?? throw new ArgumentNullException(nameof(ctx));            
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _workingDirectory = work ?? throw new ArgumentNullException(nameof(work));
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _network = new NetworkManager(_userContext);
            _home = _userContext.HomeDirectory;
            
            try
            {
                Main(args);
            }
            catch (Exception ex)
            {
                console.WriteLine("{0}: error: {1}", Name, ex.Message);
                EntryPoint.CurrentApp.Logger.LogException(ex, LogLevel.Warning);
                Complete();
            }
        }

        protected void Complete()
        {
            if (!_completed)
            {
                _completed = true;
                
                // Reset the console formatting state
                _console.WriteLine("&0");

                // dispose of the console if that's necessary.
                if (_console is IDisposable disposable && _disposeConsole)
                {
                    disposable.Dispose();
                }
            }
        }
        
        protected abstract void Main(string[] args);

        protected virtual void OnUpdate(float deltaTime)
        {
            this.Complete();
        }
        
        public void Update(float deltaTime)
        {
            if (!IsCompleted)
            {
                try
                {
                    OnUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    if (_console != null)
                    {
                        _console.WriteLine("{0}: error: {1}", Name, ex.Message);
                    }
                    else
                    {
                        EntryPoint.CurrentApp.Logger.Log(
                            "Could not log command error to the user's screen because the game killed the console instance before we got here.",
                            LogLevel.Error);
                    }

                    EntryPoint.CurrentApp.Logger.LogException(ex, LogLevel.Warning);
                    Complete();
                }
            }
        }
    }
}