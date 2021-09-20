using System;
using System.Collections.Generic;
using SociallyDistant.Editor;
using SociallyDistant.Editor.Attributes;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Core.WorldObjects
{
    [CustomView("SociallyDistant.Editors.ChatEditor")]
    public class ChatConversation : IAsset
    {
        [TweakHidden]
        public Guid Id { get; set; }
        
        [TweakName("Conversation Name")]
        [TweakDescription("The name of this conversation asset. This isn't displayed in-game.")]
        public string Name { get; set; }

        [TweakHidden] public List<ChatMessageData> Messages { get; set; } = new();
    }
}