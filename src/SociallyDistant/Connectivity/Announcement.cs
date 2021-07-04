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
            Title = json.title.rendered;
            Link = json.link;
            Excerpt = Regex.Replace(json.excerpt.rendered, "<.*?>", string.Empty);
            Date = json.date;
        }
    }
}