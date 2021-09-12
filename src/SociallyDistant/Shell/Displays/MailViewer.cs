using SociallyDistant.Core.Mail;
using SociallyDistant.Gui;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Shell.Displays
{
    [Launcher("Mail", "/icon/32/email-variant")]
    public class MailViewer : DisplayWindow
    {
        private Mailbox _mailbox;

        private Stacker _master = new();
        private Stacker _main = new();
        private TextBlock _mailboxTitle = new();
        private Stacker _mailStacker = new();
        private ScrollPanel _mailScroller = new();
        private ScrollPanel _messageScroller = new();
        private Stacker _messageStacker = new();
        private InboxEmail _message;
        private TextBlock _subject = new();
        private TextBlock _from = new();
        private Stacker _contentStacker = new();

        protected override void Main()
        {
            _mailbox = Context.Mailbox;
            
            Window.TitleText = "Mail";

            _main.Direction = StackDirection.Horizontal;
            _main.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _mailboxTitle.ForeColor = Color.Cyan;
            _mailboxTitle.Properties.SetValue(FontStyle.Heading1);
            
            _master.Children.Add(_mailboxTitle);
            _master.Children.Add(_main);
            Window.Content.Add(_master);

            _mailScroller.Children.Add(_mailStacker);
            _messageScroller.Children.Add(_messageStacker);
            
            _main.Children.Add(_mailScroller);
            _main.Children.Add(_messageScroller);
            
            _master.Padding = 48;

            _mailboxTitle.Text = "Mailbox: " + _mailbox.Address;

            ListInbox();

            _messageScroller.Visibility = Visibility.Hidden;

            _messageStacker.Children.Add(_subject);
            _messageStacker.Children.Add(_from);
            _messageStacker.Children.Add(_contentStacker);
            
            _subject.Properties.SetValue(FontStyle.Heading3);
            _subject.ForeColor = Color.Cyan;
            _from.ForeColor = Color.White * 0.5f;
            _contentStacker.Padding = new Padding(0, 12, 0, 0);

            _mailScroller.Properties.SetValue(Stacker.FillProperty, new StackFill(0.25f));
            _messageScroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _mailboxTitle.Padding = new Padding(0, 0, 0, 16);

            _subject.Padding = new Padding(15, 0, 15, 0);
            _from.Padding = _subject.Padding;
        }

        private void ListInbox()
        {
            foreach (var message in _mailbox.Inbox)
            {
                var pnl = new Panel();
                var stk = new Stacker();
                var subject = new TextBlock();
                var from = new TextBlock();

                from.Text = message.From;
                subject.Text = message.Subject;

                stk.Children.Add(subject);
                stk.Children.Add(from);

                from.ForeColor = Color.White * 0.5f;

                stk.Padding = 5;

                pnl.Children.Add(stk);

                _mailStacker.Children.Add(pnl);

                pnl.IsInteractable = true;

                pnl.BackColor = Color.FromHtml("#111111");
                pnl.MouseEnter += (o, a) =>
                {
                    pnl.BackColor = Color.FromHtml("#141414");
                };
                pnl.MouseLeave += (o, a) =>
                {
                    pnl.BackColor = Color.FromHtml("#111111");
                };

                pnl.MouseUp += (o, a) =>
                {
                    if (a.Button == MouseButton.Primary)
                    {
                        OpenEmail(message);
                    }
                };
            }
        }

        private void OpenEmail(InboxEmail email)
        {
            _messageScroller.Visibility = Visibility.Visible;
            _message = email;

            _subject.Text = email.Subject;
            _from.Text = email.From;

            _contentStacker.Children.Clear();

            var pnl = new Panel();

            _contentStacker.Children.Add(pnl);
            var stacker = new Stacker();
            stacker.Padding = 15;

            pnl.Children.Add(stacker);

            foreach (var paragraph in email.Paragraphs)
            {
                var text = new TextBlock();
                text.Text = paragraph.Text;
                text.Padding = new Padding(0, 0, 0, 6);

                stacker.Children.Add(text);

                if (paragraph.Image != null)
                {
                    var img = new Picture();
                    img.Image = paragraph.Image;

                    img.Padding = new Padding(0, 0, 0, 6);

                    stacker.Children.Add(img);
                }

            }
        }
    }
}