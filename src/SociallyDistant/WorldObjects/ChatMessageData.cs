using System;

namespace SociallyDistant.Core.ContentEditors
{
    public class ChatMessageData
    {
        public Guid From { get; set; }
        public Guid To { get; set; }
        public string Text { get; set; }
        public string AssetPath { get; set; }
        public ChatMessageType Type { get; set; }
    }
}