using System;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Core.Gui.Elements
{
    public class SocialPost : ContentElement
    {
        private TextBlock _messageText = new();
        private TextBlock _fullName = new();
        private TextBlock _username = new();
        private TextEntry _messageEditor = new();
        private Picture _avatar = new();
        private Stacker _mainStacker = new();
        private Stacker _headStacker = new();
        private Stacker _headInner = new();
        
        
        public bool IsEditor
        {
            get => _messageEditor.Visibility == Visibility.Visible;
            set
            {
                if (value)
                {
                    _messageEditor.Visibility = Visibility.Visible;
                    _messageText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _messageEditor.Visibility = Visibility.Collapsed;
                    _messageText.Visibility = Visibility.Visible;
                }
            }
        }

        public string MessageText
        {
            get => _messageText.Text;
            set
            {
                _messageText.Text = value;
                _messageEditor.Text = value;
            }
        }

        public Texture2D Avatar
        {
            get => _avatar.Image;
            set => _avatar.Image = value;
        }

        public string FullName
        {
            get => _fullName.Text;
            set => _fullName.Text = value;
        }

        public string Username
        {
            get => _username.Text;
            set => _username.Text = value;
        }

        public SocialPost()
        {
            IsEditor = false;

            _fullName.Properties.SetValue(FontStyle.Heading3);
            _username.ForeColor = Color.Gray;

            _headInner.Padding = new Padding(5, 0, 0, 0);
            _headInner.VerticalAlignment = VerticalAlignment.Center;
            _avatar.VerticalAlignment = VerticalAlignment.Center;
            
            _headStacker.Direction = StackDirection.Horizontal;
            _headStacker.Padding = new Padding(0, 0, 0, 7.5f);

            _avatar.FixedWidth = 48;
            _avatar.FixedHeight = 48;
            _avatar.ImageMode = ImageMode.Rounded;

            _headInner.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _headInner.Children.Add(_fullName);
            _headInner.Children.Add(_username);
            _headStacker.Children.Add(_avatar);
            _headStacker.Children.Add(_headInner);
            _mainStacker.Children.Add(_headStacker);
            _mainStacker.Children.Add(_messageText);
            _mainStacker.Children.Add(_messageEditor);
            Children.Add(_mainStacker);
            
            _messageEditor.TextChanged += MessageEditorOnTextChanged;
        }

        private void MessageEditorOnTextChanged(object? sender, EventArgs e)
        {
            if (IsEditor)
            {
                _messageText.Text = _messageEditor.Text;
                MessageChanged?.Invoke(this, e);
            }
        }

        public event EventHandler MessageChanged;
    }
}