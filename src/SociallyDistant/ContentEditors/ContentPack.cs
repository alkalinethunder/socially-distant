using System.IO;
using System.Text.Json;
using LiteDB;
using SociallyDistant.WorldObjects;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SociallyDistant.ContentEditors
{
    public class ContentPack
    {
        private string _id;
        private string _jsonPath;
        private string _dbPath;

        public ContentPack(string id, string json, string db)
        {
            _id = id;
            _jsonPath = json;
            _dbPath = db;
        }

        public string Id => _id;

        public ContentPackMetadata GetMetadata()
        {
            var file = File.ReadAllText(_jsonPath);
            return JsonSerializer.Deserialize<ContentPackMetadata>(file, new JsonSerializerOptions
            {
                IncludeFields = true
            });
        }

        public void SetMetadata(ContentPackMetadata metadata)
        {
            var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = true
            });
            File.WriteAllText(_jsonPath, json);
        }

        public LiteDatabase OpenData()
        {
            return new LiteDatabase(_dbPath);
        }
    }
}