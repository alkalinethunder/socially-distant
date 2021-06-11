using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Thundershock;
using Thundershock.Debugging;

namespace RedTeam.Connectivity
{
    public class AnnouncementManager : GlobalComponent
    {
        private AnnouncementState _state;
        private WebClient _webClient;
        private Task _downloadTask;
        private AnnouncementJson _announcement;
        private Announcement _shownAnnouncement;

        public Announcement Announcement => _shownAnnouncement;
        public bool IsReady => Announcement != null && _state == AnnouncementState.Ready;
        
        protected override void OnLoad()
        {
            _state = AnnouncementState.PreInit;

            _webClient = new WebClient();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            switch (_state)
            {
                case AnnouncementState.PreInit:
                    var url = "https://aklnthndr.dev/wp-json/wp/v2/posts?per_page=1&categories=24";
                    _webClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
                    _webClient.DownloadStringCompleted += WebClientOnDownloadStringCompleted;
                    _webClient.DownloadStringAsync(new Uri(url));
                    _state = AnnouncementState.Checking;
                    break;
                case AnnouncementState.Done:
                    _shownAnnouncement = new Announcement(_announcement);
                    App.Logger.Log("Retrieved announcement data.");
                    App.Logger.Log(
                        $"Title: {_shownAnnouncement.Title} ({_shownAnnouncement.Link}, {_shownAnnouncement.Date})");
                    _state = AnnouncementState.Ready;
                    break;
            }
            
            base.OnUpdate(gameTime);
        }

        private void WebClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                App.Logger.Log("Couldn't fetch community announcement.", LogLevel.Warning);
                App.Logger.LogException(e.Error);
                _state = AnnouncementState.Offline;
                return;
            }

            if (e.Cancelled)
            {
                App.Logger.Log("Announcement download was cancelled somehow. Cosmic rift maybe?");
                _state = AnnouncementState.Offline;
                return;
            }

            var json = e.Result;

            try
            {
                var announcementArray = JsonSerializer.Deserialize<AnnouncementJson[]>(json);

                // If we get null then something seriously broke on the server.
                // Either that or it no longer runs WordPress.
                //
                // If we get an empty array it's a special case, I haven't posted announcements yet.
                if (announcementArray == null || !announcementArray.Any())
                {
                    _state = AnnouncementState.Offline;
                    return;
                }
                
                // Get the first announcement.
                var announcement = announcementArray.First();
                
                // Try to read the announcement cache.
                if (TryReadAnnouncementCache(out AnnouncementJson oldAnnouncement))
                {
                    // Check the two post dates.
                    // If the new post is actually old then we won't display it to the user.
                    var old = oldAnnouncement.date;
                    var newDate = announcement.date;
                    if (old >= newDate)
                    {
                        _announcement = oldAnnouncement;
                        _state = AnnouncementState.Done;
                        return;
                    }
                }

                _announcement = announcement;
                _state = AnnouncementState.Done;
            }
            catch (Exception ex)
            {
                App.Logger.Log("Failed to parse announcement data from server.");
                App.Logger.LogException(ex);
            }
        }

        private bool TryReadAnnouncementCache(out AnnouncementJson announcement)
        {
            var path = Path.Combine(ThundershockPlatform.LocalDataPath, "announcement.cache");

            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    announcement = JsonSerializer.Deserialize<AnnouncementJson>(json);
                    return true;
                }
                catch (Exception ex)
                {
                    App.Logger.Log("Couldn't read announcement cache:", LogLevel.Warning);
                    App.Logger.LogException(ex);
                }
            }

            announcement = null;
            return false;
        }
        
        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            App.Logger.Log(
                $"Downloading {e.ProgressPercentage}%... ({e.BytesReceived} bytes / {e.TotalBytesToReceive} bytes)");
        }
    }
}