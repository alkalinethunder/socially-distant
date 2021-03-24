using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using RedTeam.Config;

namespace RedTeam.Commands
{using System;
    
    public class ConfigCommand : Command
    {
        private Dictionary<string, Func<string, bool>> _settings = new Dictionary<string, Func<string, bool>>();
        private ConfigurationManager _config;
        
        public override string Name => "set";

        private void RegisterSetting(string name, Func<string, bool> function)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Setting name must not be whitespace.");

            if (function == null)
                throw new ArgumentNullException(nameof(function));

            if (_settings.ContainsKey(name))
            {
                _settings[name] = function;
            }
            else
            {
                _settings.Add(name, function);
            }
        }

        private bool SetBoolean(ref bool setting, string value)
        {
            var result = false;
            if (value.ToLower() == "on")
            {
                setting = true;
                result = true;
            }
            else if (value.ToLower() == "off")
            {
                setting = false;
                result = true;
            }
            else if (bool.TryParse(value, out setting))
            {
                result = true;
            }
            else
            {
                Console.WriteLine("{0}: {1}: expected on|off|true|false", Name, value);
            }

            return result;
        }

        private bool SetResolution(string value)
        {
            if (value.ToLower() == "default")
            {
                _config.ActiveConfig.Resolution = string.Empty;
                return true;
            }

            try
            {
                _config.SetDisplayMode(value);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        
        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: usage: {0} <setting> <value> - type '{0} help' for a list of settings.", Name);
                return;
            }
            
            var config = RedTeamGame.Instance.GetComponent<ConfigurationManager>();

            _config = config;
            
            RegisterSetting("wm.fullscreen", (value) => SetBoolean(ref config.ActiveConfig.IsFullscreen, value));
            RegisterSetting("wm.vsync", (value) => SetBoolean(ref config.ActiveConfig.VSync, value));
            RegisterSetting("wm.fixedTimeStep",
                (value) => SetBoolean(ref config.ActiveConfig.FixedTimeStepping, value));
            RegisterSetting("wm.resolution", SetResolution);
            RegisterSetting("input.swapMouseButtons",
                (value) => SetBoolean(ref config.ActiveConfig.SwapMouseButtons, value));
            RegisterSetting("wm.effects.bloom", (value) => SetBoolean(ref config.ActiveConfig.Effects.Bloom, value));
            
            var setting = args.First();
            if (setting == "help")
            {
                foreach (var key in _settings.Keys)
                {
                    Console.WriteLine(" - {0} {1}", Name, key);
                }
            }
            else
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("{0}: usage: {0} {1}", Name, setting);
                    return;
                }

                var value = args[1];

                if (!_settings.ContainsKey(setting))
                {
                    Console.WriteLine("{0}: Unknown setting '{1}'. Type {0} help for a list of available settings.",
                        Name, setting);
                    return;
                }

                var settingFunction = _settings[setting];

                var result = settingFunction(value);

                if (result)
                {
                    config.ApplyChanges();
                }
                else
                {
                    config.DiscardChanges();
                    Console.WriteLine("Couldn't update that setting.");
                }

            }
        }
    }
}