using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace SociallyDistant.Core
{
    public static class NotificationManager
    {
        private static ConcurrentQueue<Notification> _notifications = new();

        public static bool TryGetNotification(out Notification notification)
        {
            return _notifications.TryDequeue(out notification);
        }

        public static Notification CreateNotification(string title, string message, double time = 1.5)
        {
            var note = new Notification(title, message, time, null);

            _notifications.Enqueue(note);

            return note;
        }
        
        
    }
}