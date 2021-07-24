using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using SociallyDistant.Core;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.ContentEditors;
using SociallyDistant.Core.Gui.Elements;
using SociallyDistant.Core.Windowing;
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
        
        protected override void OnLoad()
        {
            Gui.LoadStyle<HackerStyle>();
            
            BuildGui();
            
            ContentController.Init(this);
        }

        private void BuildGui()
        {
            _newProject.Activated += NewProjectOnActivated;
            
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
            _fileMenu.Items.Add(_recentProjects);
            _fileMenu.Items.Add(_exit);

            _masterMenu.Items.Add(_fileMenu);
            _masterMenu.Items.Add(_editMenu);
            _masterMenu.Items.Add(_projectMenu);
            _masterMenu.Items.Add(_helpMenu);

            _contentStacker.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _goodiesScroller.Padding = 5;
            _goodiesScroller.MinimumWidth = 375;
            
            _contentStacker.Direction = StackDirection.Horizontal;

            _editorPanel.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _goodiesStacker.Children.Add(_goodiesTitle);
            _goodiesStacker.Children.Add(_goodieTree);
            _goodiesScroller.Children.Add(_goodiesStacker);
            _goodiesPanel.Children.Add(_goodiesScroller);
            _contentStacker.Children.Add(_goodiesPanel);
            _editStacker.Children.Add(_editItems);
            _editorScroller.Children.Add(_editStacker);
            _editorPanel.Children.Add(_editorScroller);
            _contentStacker.Children.Add(_editorPanel);
            _mastStacker.Children.Add(_masterMenu);
            _mastStacker.Children.Add(_contentStacker);
            Gui.AddToViewport(_mastStacker);
        }

        private void NewProjectOnActivated(object? sender, EventArgs e)
        {
            ContentController.NewProject();
        }

        public void UpdateMenu()
        {
            // Update the recents list.
            _recentProjects.Items.Clear();

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
            if (ContentController.AssetTypes.Any())
            {
                foreach (var assetType in ContentController.AssetTypes)
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

            _editItems.Children.Add(text);
            _editItems.Children.Add(stacker);
        }

        public void AddEditItem(string category, string name, string desc)
        {
            var stacker = _categories[category];

            var hStacker = new Stacker();
            hStacker.Direction = StackDirection.Horizontal;
            hStacker.Padding = 5;
            
            var nameText = new TextBlock();
            nameText.Text = name;

            hStacker.ToolTip = desc;
            
            nameText.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            hStacker.Children.Add(nameText);
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
                
                text.Text = assetType.Name;

                _goodieTree.Children.Add(text);
                _goodieTree.Children.Add(stacker);

                _goodiesLists.Add(assetType, stacker);
            }
        }
    }
}