using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Config;

namespace RedTeam.Commands
{using System;
    
    public class ConfigCommand : Command
    {
        private List<Setting> _settings = new List<Setting>();
        private ConfigurationManager _config;

        public override string Description => "redwm configuration utility.";
        
        private string _usage = @"usage:
    {0} help                    - show this screen
    {0} set <setting> <value>   - set the value of a given setting
    {0} get <setting>           - print the value of a given setting
    {0} reset                   - load the default settings
    {0} display-modes           - print a list of available display modes
    {0} get-available           - print a list of available settings
    {0} info <setting>          - print information about a setting
";
        
        public override string Name => "settings";

        private void RegisterSetting(string name, string desc, Func<string> getter, Func<string, bool> setter)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Setting name must not be whitespace.");

            if (getter == null)
                throw new ArgumentNullException(nameof(getter));

            if (setter == null)
                throw new ArgumentNullException(nameof(setter));

            var existing = _settings.FirstOrDefault(x => x.Key == name);

            if (existing != null)
            {
                existing.Description = desc;
                existing.Getter = getter;
                existing.Setter = setter;
            }

            var setting = new Setting();

            setting.Key = name;
            setting.Description = desc;
            setting.Getter = getter;
            setting.Setter = setter;
            
            _settings.Add(setting);
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

        private string GetBoolean(ref bool value)
        {
            return value ? "on" : "off";
        }

        private string GetResolution()
        {
            var res = _config.ActiveConfig.Resolution;
            if (string.IsNullOrWhiteSpace(res))
                return "default";
            return res;
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

        private void RegisterSettings()
        {
            RegisterSetting(
                "wm.resolution",
                "The display resolution used by the window manager - must be either 'default' or 'WIDTHxHEIGHT', e.x: 1920x1080. Must be supported by your graphics card.",
                GetResolution,
                SetResolution
            );

            RegisterSetting(
                "wm.fullscreen",
                "Use full-screen or windowed mode for the redwm window manager.",
                () => GetBoolean(ref _config.ActiveConfig.IsFullscreen),
                x => SetBoolean(ref _config.ActiveConfig.IsFullscreen, x)
            );
            
            RegisterSetting(
                "wm.vsync",
                "Synchronize on vertical re-trace (prevent screen-tearing and stabilize frame-rate during redwm render)",
                () => GetBoolean(ref _config.ActiveConfig.VSync),
                x => SetBoolean(ref _config.ActiveConfig.VSync, x)
            );
            
            RegisterSetting(
                "wm.fixedTimeStep",
                "Use fixed time-stepping for redwm updates",
                () => GetBoolean(ref _config.ActiveConfig.FixedTimeStepping),
                x => SetBoolean(ref _config.ActiveConfig.FixedTimeStepping, x)
            );

            RegisterSetting(
                "input.swapMouse",
                "Swap primary and secondary mouse buttons (right- or left-handed mode)",
                () => GetBoolean(ref _config.ActiveConfig.SwapMouseButtons),
                x => SetBoolean(ref _config.ActiveConfig.SwapMouseButtons, x)
            );
            
            RegisterSetting(
                "effects.bloom",
                "Enable/disable bloom effect (glowing text, saturated colors, stoner vision)",
                () => GetBoolean(ref _config.ActiveConfig.Effects.Bloom),
                x => SetBoolean(ref _config.ActiveConfig.Effects.Bloom, x)
            );
            
            RegisterSetting(
                "effects.shadowmask",
                "Enable/disable Cathode Ray Tube Shadow-mask effect",
                () => GetBoolean(ref _config.ActiveConfig.Effects.ShadowMask),
                x => SetBoolean(ref _config.ActiveConfig.Effects.ShadowMask, x)
            );
        }

        protected override void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("{0}: error: Too few arguments.", Name);
                Console.Write(_usage, Name);
                return;
            }

            RegisterSettings();
            
            var config = RedTeamGame.Instance.GetComponent<ConfigurationManager>();

            _config = config;
            
            var action = args.First();
            var actionArgs = args.Skip(1).ToArray();

            if (action == "help")
            {
                Console.Write(_usage, Name);
                return;
            }

            if (action == "reset")
            {
                Console.WriteLine("Resetting all settings to their defaults...");
                _config.ResetToDefaults();
                return;
            }

            if (action == "info")
            {
                if (!actionArgs.Any())
                {
                    Console.WriteLine("{0}: error: {1}: Too few arguments.", Name, action);
                    Console.Write(_usage, Name);
                    return;
                }

                var setting = actionArgs.First();

                if (_settings.All(x => x.Key == setting))
                {
                    Console.WriteLine("{0}: error: {1} {2}: {2} is not a recognized setting.", Name, action, setting);
                    Console.WriteLine("Use {0} get-available for a list of available settings.");
                    return;
                }

                var info = _settings.First(x => x.Key == setting);

                Console.WriteLine(info.Key);
                
                for (var i = 0; i < info.Key.Length;i++)
                    Console.Write("-");

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Description: &w{0}&W", info.Description);
                Console.WriteLine();
                Console.WriteLine("{0} set {1} <value>", Name, info.Key);
                Console.WriteLine("{0} get {1}", Name, info.Key);
                
                return;
            }
            
            if (action == "display-modes")
            {
                Console.WriteLine("Available display modes for {0}:", GraphicsAdapter.DefaultAdapter.Description);
                Console.WriteLine();

                var modes = new List<string>();
                modes.Add(
                    $"default ({GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width}x{GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height})");

                foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.OrderByDescending(x =>
                    x.Width * x.Height))
                {
                    modes.Add($"{mode.Width}x{mode.Height}");
                }

                foreach (var mode in modes)
                {
                    Console.WriteLine(" - {0}", mode);
                }

                return;
            }
            
            if (action == "get-available")
            {
                Console.WriteLine("Available settings: ");
                Console.WriteLine();
                
                var maxLen = _settings.OrderByDescending(x => x.Key.Length).First().Key.Length + 2;

                foreach (var setting in _settings.OrderBy(x=>x.Key))
                {
                    Console.Write(" - {0}", setting.Key);

                    if (!string.IsNullOrWhiteSpace(setting.Description))
                    {
                        for (var i = 0; i < maxLen - setting.Key.Length; i++)
                        {
                            Console.Write(" ");
                        }
                        
                        Console.Write( " - &w{0}&W", setting.Description);
                    }

                    Console.WriteLine();
                }
                
                return;
            }

            if (action == "get")
            {
                if (!actionArgs.Any())
                {
                    Console.WriteLine("{0}: error: {1}: Too few arguments.", Name, action);
                    Console.Write(_usage, Name);
                    return;
                }

                var setting = actionArgs.First();

                if (_settings.All(x => x.Key != setting))
                {
                    Console.WriteLine("{0}: error: {1} {2}: {2} is not a recognized setting.", Name, action, setting);
                    Console.WriteLine("Use {0} get-available to see a list of available settings.", Name);
                    return;
                }

                var info = _settings.First(x => x.Key == setting);

                var value = info.Getter();
                
                Console.Write("{0}={1}", setting, value);

                return;
            }
            
            if (action == "set")
            {
                if (actionArgs.Length < 2)
                {
                    Console.WriteLine("{0}: error: {1}: Too few arguments.", Name, action);
                    Console.Write(_usage, Name);
                    return;
                }

                var setting = actionArgs.First();
                var value = string.Join(' ', actionArgs.Skip(1).ToArray());

                if (_settings.All(x => x.Key != setting))
                {
                    Console.WriteLine("{0}: error: {1}: '{2}' is not a recognized setting.", Name, action, setting);
                    Console.WriteLine("Run {0} get-available for a list of available settings.", Name);
                    return;
                }

                var settingInfo = _settings.First(x => x.Key == setting);

                var result = settingInfo.Setter(value);

                if (result)
                {
                    config.ApplyChanges();
                }
                else
                {
                    config.DiscardChanges();
                    Console.WriteLine("{0}: error: {1}: Couldn't set '{2}'. Settings not updated.", Name, action, setting);
                }

                return;
            }

            Console.WriteLine("{0}: error: Unrecognized command: {1}", Name, action);
            Console.Write(_usage, Name);
        }

        private class Setting
        {
            public string Key;
            public Func<string, bool> Setter;
            public Func<string> Getter;
            public string Description;
        }
    }
}