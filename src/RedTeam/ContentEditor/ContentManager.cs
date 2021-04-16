using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Thundershock;
using JsonSerializer = LiteDB.JsonSerializer;

namespace RedTeam.ContentEditor
{
    public class ContentManager : GlobalComponent
    {
        private List<ContentPack> _packs = new();
        private string _contentPath;

        public IEnumerable<ContentPack> Packs => _packs;
        
        protected override void OnLoad()
        {
            _contentPath = Path.Combine(ThundershockPlatform.LocalDataPath, "editorpacks");

            if (!Directory.Exists(_contentPath))
                Directory.CreateDirectory(_contentPath);

            LoadPackData();
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

                var jsonData = System.Text.Json.JsonSerializer.Serialize(new ContentPackMetadata(),
                    new JsonSerializerOptions
                    {
                        IncludeFields = true,
                        WriteIndented = true
                    });

                    var liteDB = new LiteDB.LiteDatabase(dbStream);

                var bytes = Encoding.UTF8.GetBytes(jsonData);
                jsonStream.Write(bytes, 0, bytes.Length);

                liteDB.Dispose();

                LoadPack(id, jsonPath, dbPath);
            }
        }

        
        private void LoadPack(string dirname, string jsonPath, string dbPath)
        {
            var pack = new ContentPack(Path.Combine(_contentPath, dirname), dirname, jsonPath, dbPath);

            _packs.Add(pack);
        }
    }
}