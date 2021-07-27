using System;
using System.Collections.Generic;
using System.Linq;
using SociallyDistant.ContentEditor;
using SociallyDistant.Core.ContentEditors;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editors
{
    public class ChatEditor : AssetView<ChatConversation>
    {
        #region UI

        private ScrollPanel _scroller = new();
        private Stacker _dumbass = new();
        private Stacker _messageStacker = new();
        private Stacker _actionStacker = new();
        private Button _addMessage = new();
        private Button _addUser = new();
        
        #endregion

        #region State

        private int _state = 0;
        private List<AgentData> _availableUsers = new();

        #endregion

        protected override void OnAssetSelected()
        {
            _addMessage.Text = "Add Message";
            _addUser.Text = "Add Character";
            
            _actionStacker.Direction = StackDirection.Horizontal;
            _actionStacker.HorizontalAlignment = HorizontalAlignment.Right;
            
            _actionStacker.Children.Add(_addMessage);
            _actionStacker.Children.Add(_addUser);

            _dumbass.Children.Add(_messageStacker);
            _dumbass.Children.Add(_actionStacker);
            _scroller.Children.Add(_dumbass);
            Children.Add(_scroller);

            _availableUsers = ContentController.GetAssetsOfType<AgentData>().ToList();
            
            foreach (var message in this.Asset.Messages)
            {
                CreateMessageBubble(message);
            }
            
            _addMessage.MouseUp += AddMessageOnMouseUp;
            
            base.OnAssetSelected();
        }

        private void AddMessageOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                var msg = new ChatMessageData();
                msg.Text = "Select a character and add message text.";
                Asset.Messages.Add(msg);
                CreateMessageBubble(msg);
                NotifyAssetChanged();
            }
        }

        private void CreateMessageBubble(ChatMessageData data)
        {
            var bubble = new MessageBubble(this, data);

            _messageStacker.Children.Add(bubble);
        }

        private class MessageBubble : ContentElement
        {
            private ChatMessageData _message;
            private ChatEditor _owner;

            private Stacker _stacker = new();
            private Picture _profilePicture = new();
            private Stacker _messageStacker = new();
            private TextBlock _authorName = new();
            private TextEntry _messageText = new();
            private Picture _messageImage = new();
            

            public MessageBubble(ChatEditor owner, ChatMessageData data)
            {
                _message = data;
                _owner = owner;

                _profilePicture.FixedWidth = 32;
                _profilePicture.FixedHeight = 32;
                _profilePicture.Padding = 3;
                _messageStacker.Padding = 3;
                _profilePicture.VerticalAlignment = VerticalAlignment.Center;
                _messageStacker.VerticalAlignment = VerticalAlignment.Center;

                _stacker.Direction = StackDirection.Horizontal;

                _messageStacker.Children.Add(_authorName);
                _messageStacker.Children.Add(_messageText);
                
                _stacker.Children.Add(_profilePicture);
                _stacker.Children.Add(_messageStacker);
                
                this.Children.Add(_stacker);

                _messageText.Text = _message.Text;
                
                _messageText.TextChanged += MessageTextOnTextChanged;
            }

            private void MessageTextOnTextChanged(object? sender, EventArgs e)
            {
                _message.Text = _messageText.Text;
                _owner.NotifyAssetChanged();
            }
        }
    }
}