using System;
using System.Collections.Generic;
using SociallyDistant.ContentEditors;

namespace SociallyDistant.WorldObjects
{
    [CustomView("SociallyDistant.Editors.ChatEditor")]
    public class ChatConversation : IAsset
    {
        [EditorHidden]
        public Guid Id { get; set; }
        
        [EditorName("Conversation Name")]
        [EditorDescription("The name of this conversation asset. This isn't displayed in-game.")]
        public string Name { get; set; }

        [EditorHidden] public List<ChatMessageData> Messages { get; set; } = new();
    }
}