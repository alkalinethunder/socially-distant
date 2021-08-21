using SociallyDistant.Core.Displays;

namespace SociallyDistant.Displays
{
    [Launcher("Mail", "/icon/32/email-variant")]
    public class MailViewer : DisplayWindow
    {
        protected override void Main()
        {
            Window.TitleText = "Mail";
        }
    }
}