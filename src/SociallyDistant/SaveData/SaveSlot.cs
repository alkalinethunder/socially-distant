using System;
using SociallyDistant.ContentEditors;

namespace SociallyDistant.SaveData
{
    public class SaveSlot
    {
        public bool IsCorrupted { get; internal set; }
        public string Path { get; }
        public InstalledContentPack GameData { get; internal set; }
        public string Title { get; internal set; }
        public DateTime LastPlayed { get; internal set; }
        public DateTime Created { get; internal set; }

        public SaveSlot(string path)
        {
            Path = path;
        }
    }
}