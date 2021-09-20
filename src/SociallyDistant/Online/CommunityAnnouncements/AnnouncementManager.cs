using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Debugging;

namespace SociallyDistant.Online.CommunityAnnouncements
{
    public class AnnouncementManager
    {
        private static AnnouncementManager _instance;

        public static AnnouncementManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new();

                return _instance;
            }
        }
        
        private AnnouncementState _state;
        private WebClient _webClient;
        private AnnouncementObject[] _announcement;
        private Announcement _shownAnnouncement;

        public Announcement Announcement => _shownAnnouncement;
        public bool IsReady => Announcement != null;
        
        private AnnouncementManager()
        {
            _state = AnnouncementState.PreInit;

            _webClient = new WebClient();
        }

        internal void OnUpdate(GameTime gameTime)
        {
            switch (_state)
            {
                case AnnouncementState.PreInit:
                    var url = "https://aklnthndr.dev/api/v1/socially-distant/announcements";
                        
                    _webClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;
                    _webClient.DownloadStringCompleted += WebClientOnDownloadStringCompleted;
                    _webClient.DownloadStringAsync(new Uri(url));
                    _state = AnnouncementState.Checking;
                    break;
                case AnnouncementState.Done:
                    _shownAnnouncement = new Announcement(_announcement.First());
                    Logger.Log("Retrieved announcement data.");
                    Logger.Log(
                        $"Title: {_shownAnnouncement.Title} ({_shownAnnouncement.Link}, {_shownAnnouncement.Date})");
                    _state = AnnouncementState.Ready;
                    break;
            }
        }

        private void WebClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Log("Couldn't fetch community announcement.", LogLevel.Warning);
                Logger.Log("Falling back to the cache.", LogLevel.Warning);
                Logger.LogException(e.Error);

                if (TryReadAnnouncementCache(out var ann))
                {
                    _shownAnnouncement = new Announcement(ann);
                    _state = AnnouncementState.Ready;
                }
                else
                {
                    Logger.Log("Cache not readable, switching to offline mode.", LogLevel.Warning);
                    _state = AnnouncementState.Offline;
                }

                return;
            }

            if (e.Cancelled)
            {
                Logger.Log("Announcement download was cancelled somehow. Cosmic rift maybe?");
                _state = AnnouncementState.Offline;
                return;
            }

            var json = e.Result;

            try
            {
                var drupalResponse = JsonSerializer.Deserialize<AnnouncementObject[]>(json);

                // If we get null then something seriously broke on the server.
                // Either that or it no longer runs WordPress.
                //
                // If we get an empty array it's a special case, I haven't posted announcements yet.
                if (drupalResponse == null || !drupalResponse.Any())
                {
                    _state = AnnouncementState.Offline;
                    return;
                }
                
                // Get the first announcement.
                var announcement = drupalResponse.First();
                
                // Try to read the announcement cache.
                if (TryReadAnnouncementCache(out var oldAnnouncement))
                {
                    // Check the two post dates.
                    // If the new post is actually old then we won't display it to the user.
                    var old = oldAnnouncement.Created;
                    var newDate = announcement.Created;
                    if (old >= newDate)
                    {
                        _shownAnnouncement = new Announcement(oldAnnouncement);
                        _state = AnnouncementState.Ready;
                        return;
                    }
                }

                _announcement = drupalResponse;
                _state = AnnouncementState.Done;
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to parse announcement data from server.");
                Logger.LogException(ex);
            }
        }

        private bool TryReadAnnouncementCache(out AnnouncementObject announcement)
        {
            var path = Path.Combine(ThundershockPlatform.LocalDataPath, "announcement.cache");

            if (File.Exists(path))
            {
                try
                {
                    var json = File.ReadAllText(path);
                    announcement = JsonSerializer.Deserialize<AnnouncementObject>(json);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Log("Couldn't read announcement cache:", LogLevel.Warning);
                    Logger.LogException(ex);
                }
            }

            announcement = null;
            return false;
        }
        
        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Logger.Log(
                $"Downloading {e.ProgressPercentage}%... ({e.BytesReceived} bytes / {e.TotalBytesToReceive} bytes)");
        }
    }
}