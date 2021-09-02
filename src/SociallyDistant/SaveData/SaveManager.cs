using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SociallyDistant.ContentEditors;
using SociallyDistant.WorldObjects;
using Thundershock;
using Thundershock.Content;
using Thundershock.Core;
using Thundershock.Core.Debugging;
using Thundershock.Core.Rendering;
using Thundershock.IO;

namespace SociallyDistant.SaveData
{
    public class SaveManager : GlobalComponent
    {
        private readonly string _metadataFileName = "metadata.rtch";
        private readonly string _gameDataFileName = "database.reddb";
        private readonly byte[] _ritchie = Encoding.UTF8.GetBytes("_31hct1R");
        private ProfileSlot _slot;
        private AssetRegistry _registry;
        private InstalledContentPack _currentPack;
        private DateTime _createdDate;
        private SaveDatabase _saveDb;
        private SaveGame _currentGame;
        private string _currentSavePath;
        private ContentManager _contentManager;
        private string _savesFolder;
        private Task _preloadTask;
        private FileSystem _fs;
        private Exception _preloadException;
        public InstalledContentPack ContentPack => _currentPack;
        
        #region Events

        public event Action<RegionNetwork> RegionAdded;
        public event Action<InternetServiceProvider> IspAdded;
        public event Action<Network> NetworkAdded;
        public event Action<Device> DeviceAdded;
        public event Action<RegionNetwork, RegionNetwork> RegionLinked;
        #endregion

        public bool IsPreloading => _preloadTask != null;
        public Exception PreloadException => _preloadException;
        
        public SaveDatabase SaveDatabase => _saveDb;

        public string GameFolder => _currentSavePath;
        
        public bool IsLoaded
            => _currentGame != null;

        public bool IsPlayerReady
            => IsLoaded && _currentGame.HasPlayer;

        public SaveGame CurrentGame => _currentGame;

        public bool HasAnySaves => _saveDb.CareerSlots.Any();
        
        public void LoadMostRecentSave()
        {
            ThrowIfLoaded();
            ThrowIfPreloading();

            var recentSave = _saveDb.CareerSlots.OrderByDescending(x => x.LastPlayDate).First();

            LoadGame(recentSave);
        }

        public void CreateNew()
        {
            // TODO: Separate base world from full-game career.
            NewGame(_contentManager.CareerPack);
        }
        
        public AssetRegistry GetAssetRegistry()
        {
            ThrowIfNotLoaded();
            ThrowIfPreloading();

            return _registry;
        }

        private void ThrowIfPreloading()
        {
            if (IsPreloading)
                throw new InvalidOperationException(
                    "Cannot perform this operation while world assets are still being loaded.");
        }
        
        private void ThrowIfLoaded()
        {
            if (IsLoaded) throw new InvalidOperationException("A save game has already been loaded.");
        }

        private void ThrowIfNotLoaded()
        {
            if (!IsLoaded) throw new InvalidOperationException("Save game is not loaded.");
        }
        
        private string GetSlotId()
        {
            var path = Path.Combine(ThundershockPlatform.LocalDataPath, "saves");
            
            var prefix = "SARS-CoV-"; // What? The game is LITERALLY called "Socially Distant."
            var id = 1;

            while (Directory.Exists(Path.Combine(path, prefix + id.ToString())))
                id++;

            return prefix + id.ToString();
        }
        
        public void NewGame(InstalledContentPack contentPack)
        {
            // Throw if the content pack is null.
            if (contentPack == null) throw new ArgumentNullException(nameof(contentPack));
            
            // Throw if there's already a game active.
            ThrowIfLoaded();
            
            // Create the slot.
            var slot = new ProfileSlot();

            slot.IsCareerMode = contentPack == _contentManager.CareerPack;
            if (!slot.IsCareerMode)
            {
                slot.RelativeCustomPackPath = contentPack.PakName;
            }

            slot.SaveSlotName = GetSlotId();

            slot.CreationDate = DateTime.Now;
            slot.LastPlayDate = DateTime.Now;

            if (slot.IsCareerMode)
            {
                _saveDb.AddCareerSave(slot);
            }
            
            // Determine the pack's data folder.
            var gameDataPath = ThundershockPlatform.LocalDataPath;
            var saveDataPath = Path.Combine(gameDataPath, "saves");

            // create parent directories.
            if (!Directory.Exists(saveDataPath))
                Directory.CreateDirectory(saveDataPath);
            
            // this is the save file.
            var saveSlotPath = Path.Combine(saveDataPath, slot.SaveSlotName);

            // Create it.
            if (!Directory.Exists(saveSlotPath))
                Directory.CreateDirectory(saveSlotPath);
            
            // Now that we have the world.pak written, we're going to open it.
            var world = slot.GetWorldDataPath();
            var pak = PakUtils.OpenPak(world);
            var fs = FileSystem.FromPak(pak);

            // Store the slot.
            _slot = slot;
            
            // Store the save slot so we can work with it later.
            _currentSavePath = saveSlotPath;

            // Now we get to set up the new game.
            // We need to create the initial world data for the player, which is frustratingly annoying, however
            // most of that's taken care of by oobe. We just need to make sure that we flag the save as new.
            _currentGame = new();
            _currentGame.IsNewGame = true;

            // Creation date is now.
            _createdDate = DateTime.Now;
            _currentPack = contentPack;
            
            // Save the game.
            Save();
            
            // Store the asset fs, and start preload.
            _fs = fs;
            _preloadTask = StartPreloadBackgroundTask();
        }

        public void DisarmPreloaderCrash()
        {
            _preloadException = null;
        }
        
        public void Save()
        {
            ThrowIfNotLoaded();

            App.Logger.Log("Saving the game...", LogLevel.Message);
            
            // Serialize and save the game state.
            var json = JsonSerializer.Serialize(_currentGame);
            var path = Path.Combine(_currentSavePath, "savedata.json");
            File.WriteAllText(path, json);
            
            // Update the slot.
            _slot.PlayerName = _currentGame.HasPlayer ? _currentGame.PlayerName : "New Game";
            _slot.LastPlayDate = DateTime.Now;
            
            // Hash the game state.
            var hash = Crypto.Sha256Hash(Encoding.UTF8.GetBytes(json));
            _slot.PlayerDataHash = hash;
            
            // Save the changes made to the slot.
            _saveDb.UpdateSlot(_slot);

            App.Logger.Log("Completed.", LogLevel.Message);
        }

        public void LoadGame(string slotId)
        {
            var slot = _saveDb.FindGame(slotId);
            LoadGame(slot);
        }

        public void LoadGame(ProfileSlot slot)
        {
            ThrowIfPreloading();
            ThrowIfLoaded();
            
            // Store the current slot.
            _slot = slot;
            
            // Set up the easy stuff.
            _createdDate = slot.CreationDate;
            _currentPack = _contentManager.GetPackInfo(slot);
            
            // The save folder.
            _currentSavePath = Path.Combine(ThundershockPlatform.LocalDataPath, "saves", slot.SaveSlotName);
            
            // Open up the pak file for the save.
            var world = slot.GetWorldDataPath();
            var pak = PakUtils.OpenPak(world);
            var fs = FileSystem.FromPak(pak);

            var metaPath = Path.Combine(_currentSavePath, "savedata.json");
            if (!File.Exists(metaPath))
                throw new InvalidOperationException(
                    "An attempt was made to load a save game that  does not actually have save data inside it. This is, without a doubt, caused by Michael's shitty coding.");

            var json = File.ReadAllText(metaPath);

            var hash = Crypto.Sha256Hash(Encoding.UTF8.GetBytes(json));
            if (hash != slot.PlayerDataHash) 
                throw new InvalidOperationException("This save file has been modified externally.");

            _currentGame = JsonSerializer.Deserialize<SaveGame>(json);
            
            // Start the preloader.
            _fs = fs;
            _preloadTask = StartPreloadBackgroundTask();
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            if (IsPreloading)
            {
                if (_preloadTask.Exception != null)
                {
                    _preloadException = _preloadTask.Exception;
                    _preloadTask = null;
                }
                else if (_preloadTask.IsCompleted)
                {
                    Logger.GetLogger().Log("Preloading has completed.");
                    _preloadTask = null;
                }
            }
            
            base.OnUpdate(gameTime);
        }

        protected override void OnLoad()
        {
            // Retrieve a reference to the content manager.
            _contentManager = App.GetComponent<ContentManager>();
         
            // Set up the save database.
            _saveDb = SaveDatabase.Load(_contentManager);

            // Make sure the saves directory exists.
            _savesFolder = Path.Combine(ThundershockPlatform.LocalDataPath, "saves");
            if (!Directory.Exists(_savesFolder))
                Directory.CreateDirectory(_savesFolder);
            
            // Initialize the saves database.
            // Start by going through the saves folder for all sub-folders.
            foreach (var dir in Directory.GetDirectories(_savesFolder))
            {
                var dname = Path.GetFileName(dir);
                
                // Is this the career mode folder?
                if (_contentManager.HasCareerMode && _contentManager.CareerPack.Id == dname)
                {
                    LoadCareerSaveData(dir);
                    continue;
                }
                
                // Check for a valid content pack.
                if (_contentManager.InstalledPacks.Any(x => x.Id == dname))
                {
                    var ext = new ExtensionSaveData(_contentManager.InstalledPacks.First(x => x.Id == dname));
                    LoadExtensionSaveData(dir, ext);

                    _saveDb.AddExtension(ext);
                }
            }
            
            base.OnLoad();
        }

        
        private IEnumerable<SaveSlot> LoadSaveSlots(string path)
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                App.Logger.Log("Loading data for: " + dir, LogLevel.Message);
                
                var saveSlot = new SaveSlot(dir);

                var metadataFile = Path.Combine(dir, _metadataFileName); // Red Team Character or.... Ritchie?
                var metaSig = metadataFile + ".sig";

                var database = Path.Combine(dir, _gameDataFileName);
                var dataSig = database + ".sig";
                
                // If ANY of those above paths DO NOT EXIST, the save file is corrupt.
                var paths = new[] {metadataFile, metaSig, database, dataSig};
                if (!paths.All(x => File.Exists(x)))
                {
                    App.Logger.Log("Save file is missing some data.", LogLevel.Error);
                    saveSlot.IsCorrupted = true;
                }
                else
                {
                    // Load the signature files.
                    var dataSigHash = File.ReadAllText(dataSig);
                    var metaSigHash = File.ReadAllText(metaSig);
                    
                    // Check both the files against their hashes... if they don't match, then
                    // the save file was tampered with or corrupted.
                    if (!Crypto.Sha256CompareFile(metadataFile, metaSigHash) ||
                        !Crypto.Sha256CompareFile(database, dataSigHash))
                    {
                        App.Logger.Log("Save data didn't pass signature checks... Tampered with, maybe?",
                            LogLevel.Error);
                        saveSlot.IsCorrupted = true;
                    }
                    else
                    {
                        // Read the metadata.
                        using var metadataStream = File.OpenRead(metadataFile);
                        using var binReader = new BinaryReader(metadataStream, Encoding.UTF8);
                        
                        // save title
                        saveSlot.Title = binReader.ReadString();
                        
                        // Last Played Date
                        saveSlot.LastPlayed = DateTime.FromBinary(binReader.ReadInt64());
                        
                        // created time
                        saveSlot.Created = DateTime.FromBinary(binReader.ReadInt64());
                    }
                }

                if (saveSlot.IsCorrupted)
                {
                    App.Logger.Log("Considering the save file corrupt.", LogLevel.Warning);
                }
                
                yield return saveSlot;
            }

        }
        
        private void LoadCareerSaveData(string path)
        {
        }

        private void LoadExtensionSaveData(string path, ExtensionSaveData saves)
        {
            App.Logger.Log($"Finding save files for extension: \"{saves.GameData.Name}\"", LogLevel.Message);

            foreach (var save in LoadSaveSlots(path))
            {
                App.Logger.Log(save.Path + " - detected.");
                save.GameData = saves.GameData;
                saves.AddSave(save);
            }
        }

        private Task StartPreloadBackgroundTask()
        {
            return Task.Run(() =>
            {
                // First we need to "restore" the world asset registry. This means looking at the pak's
                // assets.map file and deserializing all the world objects.  This is the most crappy part
                // of the preload process since we need to sig-check everything, and make sure we actually
                // have the c# object types for each asset (e.x, for modded shit).
                var registry = AssetRegistry.Restore(_fs);
                
                // Now we need to pre-load the textures for the world. These are things like character profile pictures, map icons,
                // etc.  The way this works is we iterate through the /img folder. Each file in there is a pre-computed Thundershock texture.
                // They're not PNGs, they're not JPEGs, they're not bitmaps. They're Thundershock textures. This gon' be fast.
                var done = new ManualResetEvent(false);     // This is for making sure we wait until textures are loaded.
                foreach (var texturePath in _fs.GetFiles("/img"))
                {
                    // Textures must be loaded on the same thread as the  renderer.
                    var texture = null as Texture2D;

                    // We'll start by opening the file.
                    using var stream = _fs.OpenFile(texturePath);
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        // Read the width and height.
                        var width = reader.ReadInt32();
                        var height = reader.ReadInt32();
                        
                        // Byte length is (width * 4) * height.
                        var byteLen = (width * 4) * height;
                        
                        // Read the pixels.
                        var pixels = reader.ReadBytes(byteLen);
                        
                        // Make sure that we create the texture ON THE RENDER THREAD.
                        EntryPoint.CurrentApp.EnqueueAction(() =>
                        {
                            var tex = new Texture2D((this.App as GraphicalAppBase).Graphics, width, height, TextureFilteringMode.Linear);
                            tex.Upload(pixels);
                            texture = tex;
                            done.Set();
                        });

                        done.WaitOne();
                        done.Reset();
                        
                        // Create an image asset with the texture in it.
                        var img = new ImageAsset(texture, texturePath);
                        
                        // Add it to the registry.
                        registry.AddImage(img);
                    }
                }

                _registry = registry;
            });
        }

        public bool GetEula(out string eulaText)
        {
            ThrowIfPreloading();
            ThrowIfNotLoaded();

            if (_fs.FileExists("/eula"))
            {
                eulaText = _fs.ReadAllText("/eula");
                return true;
            }

            eulaText = string.Empty;
            return false;
        }
        
        public Stream OpenWorldScript()
        {
            ThrowIfPreloading();

            if (_fs.FileExists("/script/world.js"))
                return _fs.OpenFile("/script/world.js");
            else return Stream.Null;
        }

        public FileSystem GetWorldData()
        {
            ThrowIfPreloading();
            ThrowIfNotLoaded();
            
            return _fs;
        }

        public string GetHomeDirectory(DeviceData deviceData)
        {
            ThrowIfPreloading();
            ThrowIfNotLoaded();

            var savePath = _currentSavePath;
            var homePath = Path.Combine(savePath, "homes");

            if (!Directory.Exists(homePath))
                Directory.CreateDirectory(homePath);

            var devHome = Path.Combine(homePath,
                deviceData.DeviceType == DeviceType.Player ? "player" : deviceData.Id.ToString());

            if (!Directory.Exists(devHome))
                Directory.CreateDirectory(devHome);

            return devHome;
        }
    }
}