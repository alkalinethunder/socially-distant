using System;
using System.Text.RegularExpressions;

namespace SociallyDistant.Connectivity
{
    public class Announcement
    {
        public string Title { get; }
        public string Link { get; }
        public string Excerpt { get; }
        public DateTime Date { get; }
        
        public Announcement(AnnouncementJson json)
        {
            Title = json.Title.Rendered;
            Link = json.Link;
            if (!string.IsNullOrEmpty(json.Excerpt.Rendered))
                Excerpt = Regex.Replace(json.Excerpt.Rendered, "<.*?>", string.Empty);
            else
                Excerpt = string.Empty;
            Date = json.Date;
        }
    }
}