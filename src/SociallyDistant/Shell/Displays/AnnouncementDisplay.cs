using System;
using SociallyDistant.Gui;
using SociallyDistant.Online.CommunityAnnouncements;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Shell.Displays
{
    public sealed class AnnouncementDisplay : DisplayWindow
    {
        private Stacker _master = new();
        private TextBlock _announcementTitle = new();
        private TextBlock _announcementExcerpt = new();
        private Picture _sociallyDistant = new();
        private Button _view = new();
        private Button _close = new();
        private CheckBox _showOnStartup = new();
        private Stacker _buttonStacker = new();
        private TextBlock _startupLabel = new();
        private AnnouncementManager _announcement;
        
        protected override void Main()
        {
            Window.TitleText = "Welcome - What's new";

            _announcement = AnnouncementManager.Instance;

            _view.Text = "Read more";
            _close.Text = "Close";
            _showOnStartup.Children.Add(_startupLabel);
            _startupLabel.Text = "Show on startup";
            
            _buttonStacker.Direction = StackDirection.Horizontal;
            _view.Padding = new Padding(00, 0, 2, 0);
            _close.Padding = new Padding(00, 0, 2, 0);
            _close.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _close.HorizontalAlignment = HorizontalAlignment.Left;
            _view.VerticalAlignment = VerticalAlignment.Center;
            _close.VerticalAlignment = VerticalAlignment.Center;
            _showOnStartup.VerticalAlignment = VerticalAlignment.Center;

            _master.Children.Add(_sociallyDistant);
            _master.Children.Add(_announcementTitle);
            _master.Children.Add(_announcementExcerpt);
            _master.Children.Add(_buttonStacker);
            _buttonStacker.Children.Add(_view);
            _buttonStacker.Children.Add(_close);
            _buttonStacker.Children.Add(_showOnStartup);
            Window.Content.Add(_master);

            _announcementTitle.Text = _announcement.Announcement.Title;
            _announcementExcerpt.Text = _announcement.Announcement.Excerpt;
            
            _view.MouseUp += ViewOnMouseUp;
            _close.MouseUp += CloseOnMouseUp;
            
            _sociallyDistant.HorizontalAlignment = HorizontalAlignment.Left;
            _sociallyDistant.MaximumWidth = 480;
            _announcementExcerpt.MaximumWidth = _sociallyDistant.MaximumWidth * 4;

            _announcementTitle.Properties.SetValue(FontStyle.Heading1);

            _announcementTitle.ForeColor = Color.Cyan;

            _announcementTitle.Padding = new Padding(0, 15, 0, 0);
            _buttonStacker.Padding = new Padding(0, 5, 0, 0);
            
            _master.Padding = 48;
            if (Resource.GetStream(this.GetType().Assembly, "SociallyDistant.Resources.LogoText.png", out var stream))
            {
                var texture = Texture2D.FromStream(GamePlatform.GraphicsProcessor, stream);

                _sociallyDistant.Image = texture;
                
                stream.Close();
            }
        }
        
        private void CloseOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            Window.RemoveFromParent();
        }

        private void ViewOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            ThundershockPlatform.OpenFile(_announcement.Announcement.Link);
        }
    }
}