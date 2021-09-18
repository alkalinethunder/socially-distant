using System.Collections.Generic;

namespace SociallyDistant.Core.SaveData
{
    public class SavedEmailConversation
    {
        public SavedEmail Email { get; set; }
        public List<SavedEmail> Replies { get; } = new();
    }
}