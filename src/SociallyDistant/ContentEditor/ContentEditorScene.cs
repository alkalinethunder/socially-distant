using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using SociallyDistant.Gui.Styles;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant.ContentEditor
{
    public class ContentEditorScene : Scene, IContentEditor
    {
        #region Important Shit

        public bool ShowEditor { get; set; }

        public string DataDirectory
            => Path.Combine(ThundershockPlatform.LocalDataPath, "editor");

        #endregion
        
        #region GUI Elements

        private Panel _overlay = new();
        private Stacker _mastStacker = new();
        private MenuBar _masterMenu = new();
        private Stacker _contentStacker = new();
        private Panel _goodiesPanel = new();
        private ScrollPanel _goodiesScroller = new();
        private Stacker _goodiesStacker = new();
        private TextBlock _goodiesTitle = new();
        private Stacker _goodieTree = new();
        private Panel _editorPanel = new();
        private ScrollPanel _editorScroller = new();
        private Stacker _editStacker = new();
        private Stacker _editItems = new();
        private Panel _customViewPanel = new();
        
        #endregion

        #region Editor State

        private Dictionary<string, Stacker> _categories = new();

        #endregion
        
        #region Goodies Bag State

        private Dictionary<AssetInfo, Stacker> _goodiesLists = new();

        #endregion
        
        #region Menu

        private MenuItem _projectMenu = new MenuItem("Project");
        private MenuItem _fileMenu = new MenuItem("File");
        private MenuItem _editMenu = new("Edit");
        private MenuItem _helpMenu = new MenuItem("Help");

        #endregion

        #region File Menu

        private MenuItem _newProject = new("New Project...");
        private MenuItem _openProject = new("Open Project...");
        private MenuItem _recentProjects = new("Recent Projects...");
        private MenuItem _saveProject = new("Save Project...");
        private MenuItem _packageProject = new MenuItem("Package Project...");
        private MenuItem _closeProject = new("Close Project...");
        private MenuItem _exit = new("Exit");

        #endregion

        #region Edit Menu

        private MenuItem _undo = new("Undo");
        private MenuItem _redo = new("Redo");

        #endregion

        #region Project Menu

        private MenuItem _newProjectItem = new("Create Item...");
        private MenuItem _importResource = new("Import Goodie...");
        private MenuItem _editMetadata = new("Edit Metadata...");

        #endregion

        #region Help Menu

        private MenuItem _about = new("About Socially Distant Editor");
        private MenuItem _documentation = new("Documentation...");
        private MenuItem _website = new("Website...");
        
        #endregion

        #region Image Select

        private Panel _imageSelect = new();
        private Stacker _imageSelectMain = new();
        private TextBlock _imageSelectTitle = new();
        private ScrollPanel _imageSelectScroller = new();
        private WrapPanel _imageWrapper = new();
        private Stacker _imageButtons = new();
        private Button _imageBrowse = new();
        private Action<ImageAsset> _imageCallback;
        #endregion

        #region Progress Dialog

        private Panel _progressPanel = new();
        private Stacker _progressStacker = new();
        private TextBlock _progressTitle = new();
        private TextBlock _progressStatus = new();
        private ProgressBar _progressBar = new();

        #endregion
        
        public string ImageSelectTitle
        {
            get => _imageSelectTitle.Text;
            set => _imageSelectTitle.Text = value;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            if (ContentController.IsPackaging)
            {
                _progressPanel.Visibility = Visibility.Visible;
                _progressBar.Value = ContentController.PackageProgress.Percentage;
                _progressStatus.Text = ContentController.PackageProgress.Status;
            }
            else
            {
                _progressPanel.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnLoad()
        {
            // Turn off all post-processing.
            PrimaryCameraSettings.EnableFXAA = false;
            PrimaryCameraSettings.EnableBloom = false;
            PrimaryCameraSettings.EnableCrt = false;
            
            Gui.LoadStyle<HackerStyle>();
            
            BuildGui();
            
            ContentController.Init(this);
        }


        public Visibility OverlayVisibility
        {
            get => _overlay.Visibility;
            set => _overlay.Visibility = value;
        }
        
        private void BuildGui()
        {
            _overlay.BackColor = Color.Black * 0.5f;
            _newProject.Activated += NewProjectOnActivated;
            _saveProject.Activated += SaveProjectOnActivated;
            _goodiesTitle.Text = "Goodies Bag";
            
            _helpMenu.Items.Add(_about);
            _helpMenu.Items.Add(_documentation);
            _helpMenu.Items.Add(_website);
            
            _projectMenu.Items.Add(_newProjectItem);
            _projectMenu.Items.Add(_importResource);
            _projectMenu.Items.Add(_editMetadata);
            
            _editMenu.Items.Add(_undo);
            _editMenu.Items.Add(_redo);

            _fileMenu.Items.Add(_newProject);
            _fileMenu.Items.Add(_openProject);
            _fileMenu.Items.Add(_saveProject);
            _fileMenu.Items.Add(_closeProject);
            _fileMenu.Items.Add(_packageProject);
            _fileMenu.Items.Add(_recentProjects);
            _fileMenu.Items.Add(_exit);

            _masterMenu.Items.Add(_fileMenu);
            _masterMenu.Items.Add(_editMenu);
            _masterMenu.Items.Add(_projectMenu);
            _masterMenu.Items.Add(_helpMenu);

            _contentStacker.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _goodiesScroller.Padding = 10;
            _goodiesScroller.MinimumWidth = 375;
            
            _contentStacker.Direction = StackDirection.Horizontal;

            _editorPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _editItems.Padding = 10;
            
            _goodiesStacker.Children.Add(_goodiesTitle);
            _goodiesStacker.Children.Add(_goodieTree);
            _goodiesScroller.Children.Add(_goodiesStacker);
            _goodiesPanel.Children.Add(_goodiesScroller);
            _contentStacker.Children.Add(_goodiesPanel);
            _contentStacker.Children.Add(_customViewPanel);
            _editStacker.Children.Add(_editItems);
            _editorScroller.Children.Add(_editStacker);
            _editorPanel.Children.Add(_editorScroller);
            _contentStacker.Children.Add(_editorPanel);
            _mastStacker.Children.Add(_masterMenu);
            _mastStacker.Children.Add(_contentStacker);
            Gui.AddToViewport(_mastStacker);
            Gui.AddToViewport(_overlay);
            _overlay.Visibility = Visibility.Collapsed;

            _overlay.Children.Add(_imageSelect);
            _imageSelect.HorizontalAlignment = HorizontalAlignment.Center;
            _imageSelect.VerticalAlignment = VerticalAlignment.Center;
            _imageSelect.FixedWidth = 520;
            _imageSelect.FixedHeight = 800;

            _imageBrowse.Text = "Import from Files...";

            _imageSelectTitle.Padding = 5;
            _imageWrapper.Padding = 5;
            _imageButtons.Padding = 5;
            
            _imageSelectScroller.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _imageSelectScroller.Children.Add(_imageWrapper);
            
            _imageButtons.Children.Add(_imageBrowse);
            
            _imageSelectMain.Children.Add(_imageSelectTitle);
            _imageSelectMain.Children.Add(_imageSelectScroller);
            _imageSelectMain.Children.Add(_imageButtons);
            
            _imageSelect.Children.Add(_imageSelectMain);
            
            _imageBrowse.MouseUp += ImageBrowseOnMouseUp;

            _goodiesTitle.Properties.SetValue(FontStyle.Heading2);
            _goodiesTitle.ForeColor = Color.Cyan;

            _progressPanel.FixedWidth = 600;
            _progressPanel.FixedHeight = 250;

            _progressPanel.VerticalAlignment = VerticalAlignment.Center;
            _progressPanel.HorizontalAlignment = HorizontalAlignment.Center;
            
            _progressStacker.Padding = 15;
            
            _progressTitle.Padding = new Padding(0, 0, 0, 7.5f);
            
            _progressTitle.Properties.SetValue(FontStyle.Heading3);
            _progressTitle.ForeColor = Color.Cyan;
            _progressTitle.Text = "Packaging project...";
            
            _progressTitle.TextAlign = TextAlign.Center;
            _progressStatus.TextAlign = TextAlign.Center;
            
            _progressStacker.Children.Add(_progressTitle);
            _progressStacker.Children.Add(_progressStatus);
            _progressStacker.Children.Add(_progressBar);
            _progressPanel.Children.Add(_progressStacker);
            _overlay.Children.Add(_progressPanel);

            _progressPanel.Visibility = Visibility.Collapsed;
            
            _packageProject.Activated += PackageProjectOnActivated;
            
            _editMetadata.Activated += EditMetadataOnActivated;
        }

        private void EditMetadataOnActivated(object? sender, EventArgs e)
        {
            ContentController.EditProjectMetadata();
        }

        private void ImageBrowseOnMouseUp(object? sender, MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Primary)
            {
                if (ContentController.ImportImage(this.Graphics, out var asset))
                {
                    _overlay.Visibility = Visibility.Collapsed;
                    _overlay.IsInteractable = false;
                    _imageCallback?.Invoke(asset);
                    _imageCallback = null;
                }
            }
        }

        private void SaveProjectOnActivated(object? sender, EventArgs e)
        {
            ContentController.SaveProject();
        }

        private void NewProjectOnActivated(object? sender, EventArgs e)
        {
            ContentController.NewProject();
        }

        public void UpdateMenu()
        {
            // Update the recents list.
            _recentProjects.Items.Clear();

            _saveProject.Enabled = ContentController.CanSave;

            if (ContentController.RecentProjects.Any())
            {
                foreach (var recent in ContentController.RecentProjects)
                {
                    var recentItem = new MenuItem(recent);
                    _recentProjects.Items.Add(recentItem);
                    recentItem.Activated += (o, a) =>
                    {
                        ContentController.OpenProject(recent);
                    };
                }
            }
            else
            {
                var none = new MenuItem("<empty>");
                _recentProjects.Items.Add(none);
            }
            
            // Item creation
            _newProjectItem.Items.Clear();
            var types = ContentController.AssetTypes.Where(x => x.CanUserCreate).ToArray();
            if (types.Any())
            {
                foreach (var assetType in types)
                {
                    var item = new MenuItem(assetType.Name);
                    item.Activated += (o, a) =>
                    {
                        ContentController.CreateAsset(assetType, "Unnamed " + assetType.Name);
                    };
                    _newProjectItem.Items.Add(item);
                }
            }
        }

        private void PackageProjectOnActivated(object? sender, EventArgs e)
        {
            ContentController.StartPackage();
        }

        public void SelectGoodie(IAsset asset)
        {
            foreach (var stacker in _goodiesLists.Values)
            {
                foreach (var button in stacker.Children.OfType<AdvancedButton>())
                {
                    var g = button.Properties.GetValue<IAsset>("goodie");
                    if (g == asset)
                    {
                        button.IsActive = true;
                    }
                    else
                    {
                        button.IsActive = false;
                    }
                }
            }
        }

        public bool AskForFolder(string title, out string folder)
        {
            var chooser = new FileChooser();
            chooser.FileOpenerType = FileOpenerType.FolderTree;
            chooser.Title = title;
            chooser.InitialDirectory = ThundershockPlatform.LocalDataPath;

            var result = chooser.Activate();

            if (result == FileOpenerResult.Ok)
            {
                folder = chooser.SelectedFilePath;
                return true;
            }

            folder = string.Empty;
            return false;
        }

        public void AddCategory(string name)
        {
            var stacker = new Stacker();
            _categories.Add(name, stacker);

            var text = new TextBlock();
            text.Text = name;
            text.Properties.SetValue(FontStyle.Heading3);
            text.ForeColor = Color.Cyan;

            if (_editItems.Children.Any())
            {
                text.Padding = new Padding(0, 10, 0, 5f);
            }
            else
            {
                text.Padding = new Padding(0, 0, 0, 5f);
            }


            _editItems.Children.Add(text);
            _editItems.Children.Add(stacker);
        }

        public void AddEditAction(string category, string name, string description, Action action)
        {
            var stacker = _categories[category];

            var hStacker = new Stacker();
            hStacker.Direction = StackDirection.Horizontal;
            hStacker.Padding = new Padding(10, 2);

            var spacer = new Panel();
            spacer.VerticalAlignment = VerticalAlignment.Center;
            hStacker.ToolTip = description;
            
            spacer.Properties.SetValue(Stacker.FillProperty, new StackFill(2f /  3f));
            spacer.Padding = new Padding(0, 0, 7.5f, 0);

            var button = new Button();
            button.Text = name;
            button.VerticalAlignment = VerticalAlignment.Center;
            button.HorizontalAlignment = HorizontalAlignment.Left;
            button.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            button.MouseUp += (_, a) =>
            {
                if (a.Button == MouseButton.Primary)
                {
                    action?.Invoke();
                }
            };
            
            hStacker.Children.Add(spacer);
            hStacker.Children.Add(button);

            stacker.Children.Add(hStacker);
        }

        public void AddEditItem(string category, string name, string desc, IAssetPropertyEditor editor)
        {
            var stacker = _categories[category];

            var hStacker = new Stacker();
            hStacker.Direction = StackDirection.Horizontal;
            hStacker.Padding = new Padding(10, 2);
            
            var nameText = new TextBlock();
            nameText.Text = name;
            nameText.VerticalAlignment = VerticalAlignment.Center;
            hStacker.ToolTip = desc;
            
            nameText.Properties.SetValue(Stacker.FillProperty, new StackFill(2f /  3f));
            nameText.TextAlign = TextAlign.Right;
            nameText.Padding = new Padding(0, 0, 7.5f, 0);
            
            hStacker.Children.Add(nameText);
            hStacker.Children.Add(editor.RootElement);

            editor.RootElement.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            stacker.Children.Add(hStacker);
        }

        public void ClearCategories()
        {
            _categories.Clear();
            _editItems.Children.Clear();
        }

        public void Error(string message)
        {
            DialogBox.ShowError("Error", message);
        }

        public void UpdateGoodies(AssetInfo info)
        {
            var list = _goodiesLists[info];
            list.Children.Clear();

            var goodies = ContentController.GetAssets(info);

            foreach (var goodie in goodies)
            {
                var button = new AdvancedButton();
                var text = new TextBlock();

                text.Text = goodie.Name;
                
                button.Children.Add(text);

                button.Properties.SetValue("goodie", goodie);

                button.MouseUp += (_, args) =>
                {
                    if (args.Button == MouseButton.Primary)
                    {
                        ContentController.SelectAsset(info, goodie);
                    }
                };

                list.Children.Add(button);
            }
        }

        public void ExpandGoodieCategory(AssetInfo info)
        {
            var list = _goodiesLists[info];
            list.Visibility = Visibility.Visible;
        }
        
        public void UpdateGoodieLists()
        {
            _goodiesLists.Clear();

            _goodieTree.Children.Clear();

            foreach (var assetType in ContentController.AssetTypes)
            {
                var text = new TextBlock();
                var stacker = new Stacker();
                
                text.Properties.SetValue(FontStyle.Heading3);
                text.Text = assetType.Name;
                text.Padding = new Padding(0, 7.5f, 0, 0);

                _goodieTree.Children.Add(text);
                _goodieTree.Children.Add(stacker);

                _goodiesLists.Add(assetType, stacker);
            }
        }

        public void SetCustomViewElement(Element element)
        {
            _customViewPanel.Children.Clear();

            if (element != null)
            {
                _customViewPanel.Children.Add(element);
                _customViewPanel.Visibility = Visibility.Visible;
                _customViewPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
                _editorPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Auto);
                _editorPanel.FixedWidth = 375;
            }
            else
            {
                _editorPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
                _editorPanel.FixedWidth = 0;
                _customViewPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Auto);
                _customViewPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowImageSelect(Action<ImageAsset> callback)
        {
            _overlay.Visibility = Visibility.Visible;
            _overlay.IsInteractable = true;
            _imageCallback = callback;
            
            _imageWrapper.Children.Clear();

            foreach (var img in ContentController.Images)
            {
                var button = new AdvancedButton();
                var pic = new Picture();
                
                button.FixedWidth = 160;
                button.FixedHeight = 90;
                button.Children.Add(pic);
                pic.Padding = 3;
                pic.Image = img.Texture;

                button.MouseUp += (o, a) =>
                {
                    if (a.Button == MouseButton.Primary)
                    {
                        _overlay.Visibility = Visibility.Collapsed;
                        _overlay.IsInteractable = false;
                        _imageCallback?.Invoke(img);
                        _imageCallback = null;
                    }
                };
                
                _imageWrapper.Children.Add(button);
            }
        }
    }
}