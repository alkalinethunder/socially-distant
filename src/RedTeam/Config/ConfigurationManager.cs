using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RedTeam.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics;

namespace RedTeam.Config
{
    public class ConfigurationManager : GlobalComponent
    {
        private FileSystem _fs;
        private GameConfiguration _gameConfig = null;

        public event EventHandler ConfigurationLoaded;
        
        public GameConfiguration ActiveConfig => _gameConfig;

        public IEnumerable<RedTermPalette> GetPalettes()
        {
            foreach (var path in _fs.GetFiles("/palettes"))
            {
                if (!path.ToLower().EndsWith(".json"))
                    continue;
                
                var palette = null as RedTermPalette;
                
                try
                {
                    var json = _fs.ReadAllText(path);
                    palette = JsonSerializer.Deserialize<RedTermPalette>(json, new JsonSerializerOptions
                    {
                        IncludeFields = true
                    });
                }
                catch
                {
                    // ignored
                }

                if (palette != null)
                {
                    var name = PathUtils.GetFileName(path);
                    name = name.Substring(0, name.IndexOf("."));
                    palette.id = name;
                    yield return palette;
                }
            }
        }
        
        public ColorPalette GetRedTermPalette()
        {
            var paletteName = _gameConfig.RedTermPalette;

            if (string.IsNullOrEmpty(paletteName))
                paletteName = "default";
            
            var path = $"/palettes/{paletteName}.json";

            if (_fs.FileExists(path))
            {
                try
                {
                    var json = _fs.ReadAllText(path);
                    var palette = JsonSerializer.Deserialize<RedTermPalette>(json,
                        new JsonSerializerOptions
                        {
                            IncludeFields = true
                        });

                    return palette.ToColorPalette();
                }
                catch
                {
                    return new ColorPalette();
                }
            }
            else
            {
                return new ColorPalette();
            }
        }
        
        public DisplayMode GetDisplayMode()
        {
            if (ParseDisplayMode(_gameConfig.Resolution, out int w, out int h))
            {
                var supported =
                    GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.FirstOrDefault(x =>
                        x.Width == w && x.Height == h);

                if (supported != null)
                    return supported;
            }

            return GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
        }

        public void ResetToDefaults()
        {
            _gameConfig = new GameConfiguration();
            SaveConfiguration();
            ApplyChanges();
        }
        
        public void SetDisplayMode(string value)
        {
            if (ParseDisplayMode(value, out int w, out int h))
            {
                var supported =
                    GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.FirstOrDefault(x =>
                        x.Width == w && x.Height == h);

                if (supported == null)
                    throw new InvalidOperationException(
                        $"\"{value}\" is not a display mode that is supportred by the current video card (\"{GraphicsAdapter.DefaultAdapter.Description}\").");

                _gameConfig.Resolution = $"{w}x{h}";
            }
            else
            {
                throw new InvalidOperationException(
                    $"\"{value}\" is not a properly-formatted display mode string. Must be <width>x<height>, e.x: 1920x1080");
            }
        }
        
        public void ApplyChanges()
        {
            ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
            SaveConfiguration();
        }

        public void DiscardChanges()
        {
            LoadInitialConfig();
        }

        private void WritePalette(string resourceName)
        {
            var resource = this.GetType().Assembly.GetManifestResourceStream("RedTeam.Resources.RedTermPalettes." + resourceName + ".json");
            using var reader = new StreamReader(resource, Encoding.UTF8, true);

            var json = reader.ReadToEnd();

            _fs.WriteAllText($"/palettes/" + resourceName + ".json", json);
        }

        private void WriteReadme()
        {
            var readme = this.GetType().Assembly
                .GetManifestResourceStream("RedTeam.Resources.RedTermPalettes.README.txt");
            using var reader = new StreamReader(readme, Encoding.UTF8, true);
            var readmeText = reader.ReadToEnd();

            _fs.WriteAllText("/palettes/README.txt", readmeText);
        }

        protected override void OnLoad()
        {
            // Create the local data path if it does not already exist.
            if (!Directory.Exists(RedTeamPlatform.LocalDataPath))
                Directory.CreateDirectory(RedTeamPlatform.LocalDataPath);

            // Initialize a virtual file system for that path.
            _fs = FileSystem.FromHostDirectory(RedTeamPlatform.LocalDataPath);
            
            // redterm palettes
            if (!_fs.DirectoryExists("/palettes"))
                _fs.CreateDirectory("/palettes");

            // write default palettes if they don't exist.
            WritePalette("default");
            WritePalette("light");
            WritePalette("highContrast");

            // palettes readme
            WriteReadme();
            
            // Load the initial configuration.
            LoadInitialConfig();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            
            // Save the configuration.
            SaveConfiguration();
        }

        private bool ParseDisplayMode(string displayMode, out int width, out int height)
        {
            var result = false;

            width = 0;
            height = 0;

            if (!string.IsNullOrWhiteSpace(displayMode))
            {
                var lowercase = displayMode.ToLower();

                var x = 'x';

                if (lowercase.Contains(x))
                {
                    var index = lowercase.IndexOf(x);

                    var wString = lowercase.Substring(0, index);
                    var hString = lowercase.Substring(index + 1);

                    if (int.TryParse(wString, out width) && int.TryParse(hString, out height))
                    {
                        result = true;
                    }
                }
            }
            
            return result;
        }
        
        private void SaveConfiguration()
        {
            var json = JsonSerializer.Serialize(_gameConfig, typeof(GameConfiguration), new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            });
            _fs.WriteAllText("/config.json", json);
        }
        
        private void LoadInitialConfig()
        {
            _gameConfig = null;

            if (_fs.FileExists("/config.json"))
            {
                var json = _fs.ReadAllText("/config.json");
                _gameConfig = JsonSerializer.Deserialize<GameConfiguration>(json, new JsonSerializerOptions
                {
                    IncludeFields = true
                });
            }
            else
            {
                _gameConfig = new GameConfiguration();
            }

            ConfigurationLoaded?.Invoke(this, EventArgs.Empty);
        }
    }
}