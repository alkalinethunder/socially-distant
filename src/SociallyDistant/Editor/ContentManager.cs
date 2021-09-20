using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.WorldObjects;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Debugging;

namespace SociallyDistant.Editor
{
    public class ContentManager
    {
        private static ContentManager _instance;

        internal static ContentManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new();

                return _instance;
            }
        }
        
        private List<InstalledContentPack> _installedPacks = new();
        private string _contentPath;
        private string _installPath;
        private bool _hasCareerMode;
        private InstalledContentPack _career;
        
        public bool HasCareerMode => _hasCareerMode;
        public InstalledContentPack CareerPack => _career;
        public IEnumerable<InstalledContentPack> InstalledPacks => _installedPacks;
        
        private ContentManager()
        {
            _contentPath = Path.Combine(ThundershockPlatform.LocalDataPath, "editorpacks");
            _installPath = Path.Combine(ThundershockPlatform.LocalDataPath, "packs");

            if (!Directory.Exists(_contentPath))
                Directory.CreateDirectory(_contentPath);

            if (!Directory.Exists(_installPath))
                Directory.CreateDirectory(_installPath);
            
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
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "world-base.pak");

            try
            {
                if (File.Exists(path))
                {
                    _hasCareerMode = true;
                    _career = InstalledContentPack.FromPak(GamePlatform.GraphicsProcessor, path);
                }
                else
                {
                    _hasCareerMode = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Couldn't  load career mode, falling back into demo mode.", LogLevel.Error);
                Logger.LogException(ex);
                _hasCareerMode = false;
            }
        }
        
        private void LoadInstalledPacks()
        {
            // Locate career mode first.
            LocateCareerMode();

            var graphics = GamePlatform.GraphicsProcessor;
            
            foreach (var pakFile in Directory.GetFiles(_installPath))
            {
                try
                {
                    var pack = InstalledContentPack.FromPak(graphics, pakFile);
                }
                catch (Exception ex)
                {
                    Logger.Log("custom story loading warning: bad file - " + pakFile, LogLevel.Warning);
                    Logger.LogException(ex);
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
    }
}