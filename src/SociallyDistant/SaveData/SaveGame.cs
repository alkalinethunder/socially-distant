using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.Social;
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

    public class SavedEmail
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public DateTime Received { get; set; }
        public List<SavedEmailParagraph> Paragraphs { get; set; } = new();
    }

    public class SavedEmailParagraph
    {
        public string Text { get; set; }
        public string ImagePath { get; set; }
    }
    
    public class SavedEmailConversation
    {
        public SavedEmail Email { get; set; }
        public List<SavedEmail> Replies { get; } = new();
    }
    
    public class PlayerProgress
    {
        public string FullName { get; set; }
        public Guid SafehouseId { get; set; }
        public Pronoun Pronoun { get; set; }
        
        public int Arrests { get; set; }
        public int Breaches { get; set; }
        public int Evades { get; set; }
        
        public long Cash { get; set; }
        
        public int Experience { get; set; }
        public int Skill { get; set; }
        public int Level { get; set; }
    }

    public class PlayerState
    {
        private PlayerProgress _playerProgress;

        public PlayerState(PlayerProgress progress)
        {
            _playerProgress = progress;
        }
    }
}