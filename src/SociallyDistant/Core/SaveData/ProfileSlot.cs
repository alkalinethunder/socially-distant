using System;
using System.IO;
using Thundershock;

namespace SociallyDistant.Core.SaveData
{
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
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "world-base.pak");
            }
            else
            {
                return Path.Combine(ThundershockPlatform.LocalDataPath, "packs", RelativeCustomPackPath);
            }
        }
    }
}