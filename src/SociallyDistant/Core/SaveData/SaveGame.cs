using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SociallyDistant.Core.WorldObjects;

namespace SociallyDistant.Core.SaveData
{
    public class SaveGame
    {
        private PlayerProgress _progress = null;
        
        public List<SavedEmailConversation> Emails { get; set; } = new();
        
        [JsonIgnore]
        public PlayerState PlayerState
        {
            get
            {
                if (!HasPlayer)
                    throw new InvalidOperationException("Player  doesn't exist.");
                return new PlayerState(_progress);
            }
        }
        
        public AgentData PlayerAgent { get; set; } = null;
        
        public bool  IsNewGame { get; set; }

        public bool HasPlayer => _progress != null;

        [JsonIgnore]
        public string PlayerName => _progress.FullName;
        
        [JsonIgnore]
        public Pronoun PlayerPronoun => _progress.Pronoun;

        public PlayerProgress Progress { get => _progress; set => _progress = value; }
        
        public DeviceData PlayerDevice { get; set; }
        
        public void AutoCreatePlayer(string playerName, Pronoun pronoun)
        {
            if (!HasPlayer)
            {
                _progress = new PlayerProgress()
                {
                    FullName = playerName,
                    Pronoun = pronoun
                };
            }
        }
    }
}