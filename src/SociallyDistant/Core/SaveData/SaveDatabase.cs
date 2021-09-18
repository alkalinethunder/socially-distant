using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using SociallyDistant.Editor;
using Thundershock;

namespace SociallyDistant.Core.SaveData
{
    public class SaveDatabase
    {
        private List<ProfileSlot> _careerSlots = new();
        private string _path;

        public IEnumerable<ProfileSlot> CareerSlots => _careerSlots.OrderByDescending(x => x.LastPlayDate);

        private SaveDatabase() {}

        public ProfileSlot FindGame(string slotId)
        {
            return _careerSlots.First(x => x.SaveSlotName == slotId);
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
}