namespace SociallyDistant.Core.Commands
{
    public class NotifyCommand : Command
    {
        public override string Name => "notify";
        
        protected override void Main(string[] args)
        {
            var noteText = string.Join(" ", args);

            NotificationManager.CreateNotification("Shell Notification", noteText);

            Complete();
        }
    }
}