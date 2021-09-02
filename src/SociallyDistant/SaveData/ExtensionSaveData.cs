using System;
using System.Collections.Generic;
using System.Linq;
using SociallyDistant.Core.ContentEditors;

namespace SociallyDistant.Core.SaveData
{
    public class ExtensionSaveData
    {
        private List<SaveSlot> _slots = new();
        private InstalledContentPack _pack;

        public InstalledContentPack GameData => _pack;

        public void AddSave(SaveSlot slot)
        {
            _slots.Add(slot);
        }

        public IEnumerable<SaveSlot> Slots => _slots.Where(x => !x.IsCorrupted).OrderByDescending(x => x.LastPlayed);
        
        public ExtensionSaveData(InstalledContentPack pack)
        {
            _pack = pack ?? throw new ArgumentNullException(nameof(pack));
        }
    }
}