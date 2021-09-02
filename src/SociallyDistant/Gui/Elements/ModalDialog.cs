using System;
using Thundershock.Core;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Gui.Elements
{
    public class ModalDialog : LayoutElement
    {
        private Picture _infoIcon = new Picture();
        private Stacker _contentMasterStacker = new Stacker();
        private Stacker _infoStacker = new Stacker();
        private TextBlock _messageText = new TextBlock();
        private Stacker _buttonStacker = new();

        private TextEntry _textEntry = new();

        public string UserText => _textEntry.Text;

        public Action TextSubmittedAction { get; set; }
        
        public bool HasTextEntry
        {
            get => _textEntry.Visibility == Visibility.Visible;
            set => _textEntry.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
        
        public ModalDialog(string message)
        {
            _buttonStacker.HorizontalAlignment = HorizontalAlignment.Center;
            _messageText.Text = message;
            _messageText.ForeColor = Color.Cyan;
            _messageText.MaximumWidth = 620;
            _messageText.VerticalAlignment = VerticalAlignment.Center;
            _messageText.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _infoIcon.FixedWidth = 48;
            _infoIcon.FixedHeight = 48;
            _infoIcon.VerticalAlignment = VerticalAlignment.Center;
            
            _infoStacker.Direction = StackDirection.Horizontal;

            _contentMasterStacker.Children.Add(_infoStacker);
            _contentMasterStacker.Children.Add(_textEntry);
            _contentMasterStacker.Children.Add(_buttonStacker);
            _infoStacker.Children.Add(_infoIcon);
            _infoStacker.Children.Add(_messageText);
            
            Children.Add(_contentMasterStacker);
            
            _messageText.WrapMode = TextWrapMode.WordWrap;
            
            _textEntry.Visibility = Visibility.Collapsed;
            _textEntry.TextCommitted += (_, _) =>
            {
                TextSubmittedAction?.Invoke();
            };
            
            _buttonStacker.Direction = StackDirection.Horizontal;

            _infoIcon.Padding = 3;
            _messageText.Padding = 3;
            
            _contentMasterStacker.Padding = 15;
        }

        public void AddButton(string text, Action action)
        {
            var btn = new Button();
            btn.Text = text;

            btn.Padding = new Padding(2, 0);
            
            btn.MouseUp += (_, _) => action?.Invoke();
            
            _buttonStacker.Children.Add(btn);
        }
    }
}