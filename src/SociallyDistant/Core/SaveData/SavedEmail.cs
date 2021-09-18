using System;
using System.Collections.Generic;

namespace SociallyDistant.Core.SaveData
{
    public class SavedEmail
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public DateTime Received { get; set; }
        public List<SavedEmailParagraph> Paragraphs { get; set; } = new();
    }
}