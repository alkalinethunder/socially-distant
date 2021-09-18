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

namespace SociallyDistant.Core.Config
{
    public class RedConfigManager : GlobalComponent
    {
        private FileSystem _fs;
        private GameConfig _config;
        
        public event EventHandler ConfigUpdated;
        
        public GameConfig ActiveConfig
            => _config;
        
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
        
        protected override void OnLoad()
        {
            _fs = FileSystem.FromHostDirectory(ThundershockPlatform.LocalDataPath);
            
            // load initial config.
            LoadInitialConfig();
            
            base.OnLoad();
        }
        
        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(_config, typeof(GameConfig), new JsonSerializerOptions
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
                _config = JsonSerializer.Deserialize<GameConfig>(json, new JsonSerializerOptions
                {
                    IncludeFields = true
                });
            }
            else
            {
                _config = new GameConfig();
                SaveConfig();
            }
        }
    }
}