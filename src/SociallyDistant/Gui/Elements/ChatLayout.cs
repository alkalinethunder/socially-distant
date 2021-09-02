using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class ChatLayout : LayoutElement
    {
        private Panel _panel = new();
        private Stacker _stacker = new();
        private ScrollPanel _mainScroller = new();
        private Stacker _messageStacker = new();
        private Stacker _headStacker = new();
        private Picture _convoAvatar = new();
        private TextBlock _convoName = new();
        private TextBlock _participants = new();
        private Stacker _convoInfoStacker = new();

        public string ConversationName
        {
            get => _convoName.Text;
            set => _convoName.Text = value;
        }

        public string ParticipantsText
        {
            get => _participants.Text;
            set => _participants.Text = value;
        }

        public Texture2D ConversationImage
        {
            get => _convoAvatar.Image;
            set => _convoAvatar.Image = value;
        }

        public ChatLayout()
        {
            _convoName.ForeColor = Color.White;
            _participants.ForeColor = Color.FromHtml("#dadada");

            _headStacker.Padding = 10;
            _convoInfoStacker.Padding = new Padding(5, 0, 0, 0);
            _convoInfoStacker.VerticalAlignment = VerticalAlignment.Center;
            
            _convoAvatar.VerticalAlignment = VerticalAlignment.Center;

            _convoAvatar.FixedWidth = 48;
            _convoAvatar.FixedHeight = 48;
            _convoAvatar.ImageMode = ImageMode.Rounded;

            _headStacker.Direction = StackDirection.Horizontal;
            
            _convoInfoStacker.Children.Add(_convoName);
            _convoInfoStacker.Children.Add(_participants);
            _headStacker.Children.Add(_convoAvatar);
            _headStacker.Children.Add(_convoInfoStacker);
            _mainScroller.Children.Add(_messageStacker);
            _stacker.Children.Add(_headStacker);
            _stacker.Children.Add(_mainScroller);
            _panel.Children.Add(_stacker);
            Children.Add(_panel);
        }

        public void ClearMessages()
        {
            _messageStacker.Children.Clear();
        }

        public ChatBubble AddMessage(string messageText, bool isFromPlayer = false)
        {
            var bubble = new ChatBubble(messageText, isFromPlayer);
            
            _messageStacker.Children.Add(bubble);

            return bubble;
        }
    }
}