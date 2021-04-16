namespace RedTeam.ContentEditor
{
    public class ContentPack
    {
        private string _id;
        private string _path;
        private string _jsonPath;
        private string _dbPath;

        public ContentPack(string path, string id, string json, string db)
        {
            _path = path;
            _id = id;
            _jsonPath = json;
            _dbPath = db;
        }

        public string Id => _id;
    }

    public class ContentPackMetadata
    {
        public string name;
        public string author;
        public string description;
        public string iconPath;
        public string wallpaperPath;
    }
}