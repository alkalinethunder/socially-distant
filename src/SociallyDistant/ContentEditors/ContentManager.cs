using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using SociallyDistant.Core.SaveData;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Debugging;

namespace SociallyDistant.Core.ContentEditors
{
    public class ContentManager : GlobalComponent
    {
        private List<InstalledContentPack> _installedPacks = new();
        private List<ContentPack> _packs = new();
        private string _contentPath;
        private string _installPath;
        private bool _hasCareerMode;
        private InstalledContentPack _career;
        
        public bool HasCareerMode => _hasCareerMode;
        public InstalledContentPack CareerPack => _career;
        public IEnumerable<ContentPack> Packs => _packs;
        public IEnumerable<InstalledContentPack> InstalledPacks => _installedPacks;
        
        protected override void OnLoad()
        {
            _contentPath = Path.Combine(ThundershockPlatform.LocalDataPath, "editorpacks");
            _installPath = Path.Combine(ThundershockPlatform.LocalDataPath, "packs");

            if (!Directory.Exists(_contentPath))
                Directory.CreateDirectory(_contentPath);

            if (!Directory.Exists(_installPath))
                Directory.CreateDirectory(_installPath);
            
            LoadPackData();
            LoadInstalledPacks();
        }

        private void LocateCareerMode()
        {
            // CHANGES FOR THE NEW PAK-BASED WORLD SYSTEM
            //
            // Instead of the career being inside a specific library from the closed-source prtion of the game,
            // it is now stored in the program root. In the future it will be stored inside a thundershock
            // secure pak but that behaviour isn't yet possible. For the time being it's not necessary
            // since this is a very early version of the game.
            // 
            // So we'll just look for career.pak and try to load it.
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "career.pak");

            try
            {
                if (File.Exists(path))
                {
                    _hasCareerMode = true;
                    _career = InstalledContentPack.FromPak((App as GraphicalAppBase).Graphics, path);
                }
                else
                {
                    _hasCareerMode = false;
                }
            }
            catch (Exception ex)
            {
                App.Logger.Log("Couldn't  load career mode, falling back into demo mode.", LogLevel.Error);
                App.Logger.LogException(ex);
                _hasCareerMode = false;
            }
        }
        
        private void LoadInstalledPacks()
        {
            // Locate career mode first.
            LocateCareerMode();

            var graphics = (App as GraphicalAppBase).Graphics;
            
            foreach (var pakFile in Directory.GetFiles(_installPath))
            {
                try
                {
                    var pack = InstalledContentPack.FromPak(graphics, pakFile);
                }
                catch (Exception ex)
                {
                    App.Logger.Log("custom story loading warning: bad file - " + pakFile, LogLevel.Warning);
                    App.Logger.LogException(ex);
                }
            }
        }
        
        private void LoadPackData()
        {
            foreach (var dir in Directory.GetDirectories(_contentPath))
            {
                var dirname = Path.GetFileName(dir);
                var jsonPath = Path.Combine(dir, "redteam.meta");
                var dbPath = Path.Combine(dir, "content.db");
                if (File.Exists(jsonPath) && File.Exists(dbPath))
                {
                    LoadPack(dirname, jsonPath, dbPath);
                }
            }
        }

        public InstalledContentPack GetPackInfo(ProfileSlot slot)
        {
            if (slot.IsCareerMode)
            {
                return _career;
            }
            else
            {
                return _installedPacks.First(x => x.PakName == slot.RelativeCustomPackPath);
            }
        }
        
        public void CreatePack(string id)
        {
            if (!_packs.Any(x => x.Id == id))
            {
                var fullPath = Path.Combine(_contentPath, id);
                Directory.CreateDirectory(fullPath);
                    
                var jsonPath = Path.Combine(fullPath, "redteam.meta");
                var dbPath = Path.Combine(fullPath, "content.db");

                using var dbStream = File.Create(dbPath);
                using var jsonStream = File.Create(jsonPath);

                var jsonData = JsonSerializer.Serialize(new ContentPackMetadata(),
                    new JsonSerializerOptions
                    {
                        IncludeFields = true,
                        WriteIndented = true
                    });

                    var liteDb = new LiteDB.LiteDatabase(dbStream);

                var bytes = Encoding.UTF8.GetBytes(jsonData);
                jsonStream.Write(bytes, 0, bytes.Length);

                liteDb.Dispose();

                LoadPack(id, jsonPath, dbPath);
            }
        }

        
        private void LoadPack(string dirname, string jsonPath, string dbPath)
        {
            var pack = new ContentPack(dirname, jsonPath, dbPath);

            _packs.Add(pack);
        }
    }
}