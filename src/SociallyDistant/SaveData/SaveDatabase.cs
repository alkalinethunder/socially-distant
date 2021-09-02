using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using SociallyDistant.Core.ContentEditors;
using Thundershock;

namespace SociallyDistant.Core.SaveData
{
    public class SaveDatabase
    {
        private List<ProfileSlot> _careerSlots = new();
        private readonly List<ExtensionSaveData> _extensions = new();
        private string _path;

        public IEnumerable<ProfileSlot> CareerSlots => _careerSlots.OrderByDescending(x => x.LastPlayDate);

        private SaveDatabase() {}

        public ProfileSlot FindGame(string slotId)
        {
            return _careerSlots.First(x => x.SaveSlotName == slotId);
        }
        
        public IEnumerable<SaveSlot> GetExtensionSaves(InstalledContentPack pack)
        {
            return _extensions.First(x => x.GameData == pack).Slots;
        }

        public void UpdateSlot(ProfileSlot slot)
        {
            using var ldb = new LiteDatabase(_path);
            var slotCollection = ldb.GetCollection<ProfileSlot>(nameof(ProfileSlot));

            slot.IsCareerMode = true;

            slotCollection.Update(slot);

            ldb.Dispose();
        }
        
        public void AddCareerSave(ProfileSlot slot)
        {
            using var ldb = new LiteDatabase(_path);
            var slotCollection = ldb.GetCollection<ProfileSlot>(nameof(ProfileSlot));

            slot.IsCareerMode = true;

            slotCollection.Insert(slot);

            ldb.Dispose();
            
            _careerSlots.Add(slot);
        }
        
        internal void AddExtension(ExtensionSaveData ext)
        {
            _extensions.Add(ext);
        }

        private void Restore(string path)
        {
            _path = path;
            
            using var ldb = new LiteDatabase(path);

            var slotCollection = ldb.GetCollection<ProfileSlot>(nameof(ProfileSlot));

            foreach (var slot in slotCollection.FindAll())
            {
                if (slot.IsCareerMode)
                {
                    _careerSlots.Add(slot);
                }
                else
                {
                    var customPack = Path.Combine(ThundershockPlatform.LocalDataPath, "packs",
                        slot.RelativeCustomPackPath);

                    if (File.Exists(customPack))
                    {
                        // TODO
                    }
                }
            }
            
            ldb.Dispose();
        }

        private void Initialize(string path)
        {
            _path = path;
            
            using var ldb = new LiteDatabase(path);

            ldb.GetCollection<ProfileSlot>(nameof(ProfileSlot));
            
            ldb.Dispose();
        }
        
        internal static SaveDatabase Load(ContentManager contentManager)
        {
            var saveDBPath = Path.Combine(ThundershockPlatform.LocalDataPath, "profile.db");

            var sdb = new SaveDatabase();
            
            if (File.Exists(saveDBPath))
            {
                sdb.Restore(saveDBPath);
            }
            else
            {
                sdb.Initialize(saveDBPath);
            }
            
            return sdb;
        }
    }

    public class ProfileSlot
    {
        public int Id { get; set; }
        
        public string SaveSlotName { get; set; }
        
        public string PlayerName { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastPlayDate { get; set; }
        
        public string PlayerDataHash { get; set; }
        
        public bool IsCareerMode { get; set; }
        public string RelativeCustomPackPath { get; set; }

        public string GetWorldDataPath()
        {
            if (IsCareerMode)
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Career.pak");
            }
            else
            {
                return Path.Combine(ThundershockPlatform.LocalDataPath, "packs", RelativeCustomPackPath);
            }
        }
    }
}