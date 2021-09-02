using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Debugging;
using Thundershock.Gui.Elements.Console;
using Thundershock.IO;

namespace SociallyDistant.Config
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

            if (_palettes.Any(x => x.Id == _config.RedTermPalette))
            {
                palette = _palettes.First(x => x.Id == _config.RedTermPalette);
            }
            else
            {
                palette = _palettes.First(x => x.Id == "default");
            }
            
            return palette.ToColorPalette();
        }

        public void SetConsoleFonts(ConsoleControl console)
        {
            var gfx = console.GuiSystem.Graphics;
            var baseResourceId = "SociallyDistant.Resources.Fonts.";
            var bold = baseResourceId + "UbuntuMono-B.ttf";
            var regular = baseResourceId + "UbuntuMono-R.ttf";
            var boldItalic = baseResourceId + "UbuntuMono-BI.ttf";
            var italic = baseResourceId + "UbuntuMono-RI.ttf";

            var b = Font.FromResource(gfx, this.GetType().Assembly, bold);
            var r = Font.FromResource(gfx, this.GetType().Assembly, regular);
            var bisexualBilly = Font.FromResource(gfx, this.GetType().Assembly, boldItalic);
            var i = Font.FromResource(gfx, this.GetType().Assembly, italic);

            var baseFontSize = 18;

            b.Size = baseFontSize;
            r.Size = b.Size;
            bisexualBilly.Size = r.Size;
            i.Size = bisexualBilly.Size;

            console.Font = r;
            console.BoldFont = b;
            console.BoldItalicFont = bisexualBilly;
            console.ItalicFont = i;
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

                palette.Id = id;
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
            var asm = GetType().Assembly;
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
            AddPaletteResource("default", "SociallyDistant.Resources.RedTermPalettes.default.json");
            AddPaletteResource("highContrast","SociallyDistant.Resources.RedTermPalettes.highContrast.json");
            AddPaletteResource("light", "SociallyDistant.Resources.RedTermPalettes.light.json");

            // palettes readme
            WriteReadme();

            // load user palettes.
            LoadUserPalettes();
            
            // load initial config.
            LoadInitialConfig();
            
            base.OnLoad();
        }

        private void LoadUserPalettes()
        {
            foreach (var file in _fs.GetFiles("/palettes"))
            {
                try
                {
                    var json = _fs.ReadAllText(file);
                    var fname = PathUtils.GetFileName(file);
                    var palette = JsonSerializer.Deserialize<RedTermPalette>(json, new JsonSerializerOptions
                    {
                        IncludeFields = true
                    });

                    palette.Id = fname;

                    if (!_palettes.Any(x => x.Name == palette.Name))
                        _palettes.Add(palette);
                }
                catch
                {
                    // ignore
                }
            }
        }
        
        private void WriteReadme()
        {
            var readme = GetType().Assembly
                .GetManifestResourceStream("SociallyDistant.Resources.RedTermPalettes.README.txt");
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