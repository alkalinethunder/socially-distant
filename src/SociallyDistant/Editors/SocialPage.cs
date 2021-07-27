using System.Linq;
using SociallyDistant.ContentEditor;
using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
using Thundershock.Gui.Elements;
using Thundershock.Core;
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
        private Picture _profilePicture = new();
        private TextBlock _fullName = new();
        private TextBlock _tag = new();
        private TextBlock _bio = new();

        #endregion

        protected override void OnAssetSelected()
        {
            _socialCover.FixedHeight = 256;
            
            _profilePicture.FixedWidth = 128;
            _profilePicture.FixedHeight = 128;
            
            _profileImageStacker.Direction = StackDirection.Horizontal;

            _profileImageStacker.Padding = new Padding(15, -64, 15, 15);
            
            _profileImageStacker.Children.Add(_profilePicture);
            _headStacker.Children.Add(_socialCover);
            _headStacker.Children.Add(_profileImageStacker);
            _headStacker.Children.Add(_fullName);
            _headStacker.Children.Add(_tag);
            _headStacker.Children.Add(_bio);
            _mainStacker.Children.Add(_headStacker);
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
    }
}