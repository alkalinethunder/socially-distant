using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Thundershock;
using Thundershock.Debugging;
using Thundershock.IO;

namespace RedTeam.Config
{
    public class RedConfigManager : GlobalComponent
    {
        private FileSystem _fs;
        private RedConfig _config;
        private List<RedTermPalette> _palettes = new List<RedTermPalette>();

        public event EventHandler ConfigUpdated;
        
        public RedConfig ActiveConfig
            => _config;

        public IEnumerable<RedTermPalette> GetPalettes()
            => _palettes;

        public ColorPalette GetPalette()
        {
            var palette = null as RedTermPalette;

            if (_palettes.Any(x => x.id == _config.RedTermPalette))
            {
                palette = _palettes.First(x => x.id == _config.RedTermPalette);
            }
            else
            {
                palette = _palettes.First(x => x.id == "default");
            }
            
            return palette.ToColorPalette();
        }
        
        public void ApplyChanges()
        {
            // save
            SaveConfig();
            
            // fire.
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void DiscardChanges()
        {
            // unload the config.
            _config = null;
            
            // re-load.
            LoadInitialConfig();
            
            // fire.
            ConfigUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void AddPaletteInternal(string id, string json)
        {
            try
            {
                var palette = JsonSerializer.Deserialize<RedTermPalette>(json, new JsonSerializerOptions
                {
                    IncludeFields = true
                });

                palette.id = id;
                _palettes.Add(palette);
            }
            catch (Exception ex)
            {
                App.Logger.Log("Could not load redterm palette: " + id, LogLevel.Warning);
                App.Logger.LogException(ex, LogLevel.Warning);
            }
        }
        
        private void AddPaletteResource(string id, string resourceId)
        {
            var asm = this.GetType().Assembly;
            var resource = asm.GetManifestResourceStream(resourceId);
            var reader = new StreamReader(resource, Encoding.UTF8, true);

            var json = reader.ReadToEnd();

            AddPaletteInternal(id, json);
        }
        
        protected override void OnLoad()
        {
            _fs = FileSystem.FromHostDirectory(ThundershockPlatform.LocalDataPath);
            
            // redterm palettes
            if (!_fs.DirectoryExists("/palettes"))
                _fs.CreateDirectory("/palettes");

            // read palettes from internal resources.
            AddPaletteResource("default", "RedTeam.Resources.RedTermPalettes.default.json");
            AddPaletteResource("highContrast","RedTeam.Resources.RedTermPalettes.highContrast.json");
            AddPaletteResource("light", "RedTeam.Resources.RedTermPalettes.light.json");

            // palettes readme
            WriteReadme();

            // load initial config.
            LoadInitialConfig();
            
            base.OnLoad();
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

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(_config, typeof(RedConfig), new JsonSerializerOptions
            {
                IncludeFields = true
            });
            _fs.WriteAllText("/redteam.json", json);
        }
        
        private void LoadInitialConfig()
        {
            if (_fs.FileExists("/redteam.json"))
            {
                var json = _fs.ReadAllText("/redteam.json");
                _config = JsonSerializer.Deserialize<RedConfig>(json, new JsonSerializerOptions
                {
                    IncludeFields = true
                });
            }
            else
            {
                _config = new RedConfig();
                SaveConfig();
            }
        }
    }
}