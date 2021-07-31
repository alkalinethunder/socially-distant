using System.Linq;
using SociallyDistant.ContentEditor;
using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.Gui.Elements;
using Thundershock.Gui.Elements;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;

namespace SociallyDistant.Editors
{
    public class SocialPage : AssetView<AgentData>
    {
        #region UI Elements
        
        private Picture _socialCover = new();
        private ScrollPanel _mainScroller = new();
        private Stacker _mainStacker = new();
        private Stacker _headStacker = new();
        private Stacker _profileImageStacker = new();
        private Stacker _postList = new();
        private Picture _profilePicture = new();
        private TextBlock _fullName = new();
        private TextBlock _tag = new();
        private TextBlock _bio = new();
        private Button _addPost = new();
        
        
        #endregion

        protected override void OnAssetSelected()
        {
            _socialCover.FixedHeight = 256;
            
            _profilePicture.FixedWidth = 128;
            _profilePicture.FixedHeight = 128;
            
            _profileImageStacker.Direction = StackDirection.Horizontal;

            _profileImageStacker.Padding = new Padding(15, -64, 15, 15);

            _addPost.VerticalAlignment = VerticalAlignment.Bottom;
            _addPost.HorizontalAlignment = HorizontalAlignment.Right;
            _addPost.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _addPost.Text = "Add Post";
            
            _profileImageStacker.Children.Add(_profilePicture);
            _profileImageStacker.Children.Add(_addPost);
            _headStacker.Children.Add(_socialCover);
            _headStacker.Children.Add(_profileImageStacker);
            _headStacker.Children.Add(_fullName);
            _headStacker.Children.Add(_tag);
            _headStacker.Children.Add(_bio);
            _mainStacker.Children.Add(_headStacker);
            _mainStacker.Children.Add(_postList);
            _postList.FixedWidth = 415;
            _postList.HorizontalAlignment = HorizontalAlignment.Left;
            _mainScroller.Children.Add(_mainStacker);
            Children.Add(_mainScroller);

            _fullName.Properties.SetValue(FontStyle.Heading2);

            _tag.ForeColor = Color.Gray;

            _fullName.Padding = new Padding(15, 0);
            _tag.Padding = new Padding(15, 0);
            _bio.Padding = new Padding(15, 7.5f, 15, 0);

            _socialCover.ImageMode = ImageMode.Zoom;
            _profilePicture.ImageMode = ImageMode.Rounded;

            _profilePicture.BorderWidth = 3;
            
            _addPost.MouseUp += AddPostOnMouseUp;

            foreach (var post in Asset.Posts)
            {
                AddSocialPOst(post);
            }
        }

        private void AddPostOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                var post = new AgentSocialPost();
                Asset.Posts.Add(post);
                NotifyAssetChanged();
                AddSocialPOst(post);
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _fullName.Text = Asset.Name;
            _tag.Text = "@" + Asset.UserName ?? Asset.Name.ToUnixUsername();
            _bio.Text = Asset.Bio ?? "This user hasn't written a bio yet.";

            if (Asset.ProfilePicture != null)
            {
                var img = ContentController.Images.FirstOrDefault(x => x.Path == Asset.ProfilePicture.Path);
                if (img != null)
                {
                    _profilePicture.Image = img.Texture;
                }
                else
                {
                    _profilePicture.Image = null;
                }
            }
            else
            {
                _profilePicture.Image = null;
            }
            
            if (Asset.CoverArt != null)
            {
                var img = ContentController.Images.FirstOrDefault(x => x.Path == Asset.CoverArt.Path);
                if (img != null)
                {
                    _socialCover.Image = img.Texture;
                }
                else
                {
                    _socialCover.Image = null;
                }
            }
            else
            {
                _socialCover.Image = null;
            }
            
            base.OnUpdate(gameTime);
        }

        private void AddSocialPOst(AgentSocialPost post)
        {
            var postGui = new SocialPost();
            postGui.FullName = Asset.Name;
            postGui.Username = "@" + Asset.UserName ?? Asset.Name.ToUnixUsername();
            postGui.MessageText = post.Text;
            postGui.IsEditor = true;
            postGui.Avatar = _profilePicture.Image;

            postGui.MessageChanged += (_, _) =>
            {
                post.Text = postGui.MessageText;
                NotifyAssetChanged();
            };
            
            
            _postList.Children.Insert(0, postGui);
        }
    }
}