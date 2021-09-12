using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SociallyDistant.Core.WorldObjects;
using Thundershock.Content;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.IO;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SociallyDistant.Editor
{
    public class InstalledContentPack
    {
        private Func<Stream> _dataReader;
        
        private string _id;
        private string _folder;
        private string _jsonPath;
        private string _savesFolder;
        
        [JsonIgnore]
        public string PakName { get; private set; }
        
        [JsonPropertyName("name")]
        public string Name { get; private set; }
        
        [JsonPropertyName("author")]
        public string Author { get; private set; }
        
        [JsonPropertyName("description")]
        public string Description { get; private set; }
        public Texture2D Backdrop { get; private set; }
        public Texture2D Icon { get; private set; }
        public Texture2D BootLogo { get; private set; }
        public bool RequiresOutOfBoxExperience => true; /* TODO */
        
        public string Id
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_id))
                {
                    foreach (var ch in Name)
                    {
                        if (char.IsLetterOrDigit(ch))
                        {
                            _id += char.ToLower(ch);
                        }
                    }
                }

                return _id;
            }
        }
        
        private InstalledContentPack() {}
        
        private InstalledContentPack(string folder, string jsonPath)
        {
            _folder = folder;
            _jsonPath = jsonPath;

            _savesFolder = Path.Combine(_folder, "saves");
        }

        public static bool TryLoadCareer(string json, Assembly assembly, string gameData, out InstalledContentPack pack)
        {
            try
            {
                var metadata = JsonSerializer.Deserialize<ContentPackMetadata>(json, new JsonSerializerOptions
                {
                    IncludeFields = true
                });

                pack = new InstalledContentPack();
                pack.Name = metadata.Name;
                pack.Author = metadata.Author;
                pack.Description = metadata.Description;
                
                pack._dataReader = () =>
                    assembly.GetManifestResourceStream(gameData);

                    return true;
            }
            catch (Exception ex)
            {
                EntryPoint.CurrentApp.Logger.Log("Cannot load career mode data.");
                EntryPoint.CurrentApp.Logger.LogException(ex);
            }
            
            pack = null;
            return false;
        }
        
        public void WriteWorld(string savePath)
        {
            using var stream = _dataReader();

            var world = Path.Combine(savePath, "world.pak");

            using var worldPak = File.OpenWrite(world);

            stream.CopyTo(worldPak);
            stream.Close();
            worldPak.Close();
        }

        public static InstalledContentPack FromPak(GraphicsProcessor gpu, string pakPath)
        {
            var pakName = Path.GetFileName(pakPath);
            var pakFile = PakUtils.OpenPak(pakPath);
            var fs = FileSystem.FromPak(pakFile);

            try
            {
                if (!fs.FileExists("/meta"))
                    throw new InvalidOperationException("Missing metadata file.");

                if (!fs.FileExists("/meta.icon"))
                    throw new InvalidOperationException("Missing world icon.");

                if (!fs.FileExists("/meta.boot"))
                    throw new InvalidOperationException("Missing boot logo.");

                if (!fs.FileExists("/wallpaper"))
                    throw new InvalidOperationException("Missing wallpaper.");

                using var metaStream = fs.OpenFile("/meta");
                using var reader = new BinaryReader(metaStream, Encoding.UTF8);

                var name = reader.ReadString();
                var author = reader.ReadString();
                var description = reader.ReadString();

                var icon = Texture2D.FromPak(gpu, fs.OpenFile("/meta.icon"));
                var wallpaper = Texture2D.FromPak(gpu, fs.OpenFile("/wallpaper"));
                var boot = Texture2D.FromPak(gpu, fs.OpenFile("/meta.boot"));

                var pack = new InstalledContentPack();
                pack.PakName = pakName;
                pack.Name = name;
                pack.Description = description;
                pack.Author = author;
                pack.Icon = icon;
                pack.Backdrop = wallpaper;
                pack.BootLogo = boot;
                pack._dataReader = () => File.OpenRead(pakPath);

                return pack;
            }
            catch (Exception ex)
            {
                pakFile.Dispose();
                throw new InvalidOperationException("Bad world pak", ex);
            }
            
            pakFile.Dispose();
            return null;
        }
    }
}