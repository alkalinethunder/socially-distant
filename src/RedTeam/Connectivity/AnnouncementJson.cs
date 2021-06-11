using System;

namespace RedTeam.Connectivity
{
    public class AnnouncementJson
    {
        public int id { get; set; }
        public DateTime date { get; set; }
        public string link { get; set; }
        public RenderedText title { get; set; }
        public RenderedText excerpt { get; set; }
        public int featured_media { get; set; }
    }
}