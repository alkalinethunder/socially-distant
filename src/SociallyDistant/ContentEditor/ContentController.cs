using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Silk.NET.OpenGL;
using SociallyDistant.AssetPropertyEditors;
using SociallyDistant.Editors;
using SociallyDistant.WorldObjects;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.IO;

namespace SociallyDistant.ContentEditor
{
    public static class ContentController
    {
        private static string _activeProjectFolder = string.Empty;
        private static IContentEditor _editor;
        private static FileSystem _projectFS;
        private static FileSystem _dataFS;
        private static string[] _recents;
        private static ContentPackMetadata _metadata = null;
        private static AssetRegistry _registry = new();
        private static Task _packageTask;
        private static PakWorker _worker;
        
        public static IEnumerable<string> RecentProjects => _recents;

        
        
        public static IEnumerable<AssetInfo> AssetTypes => _registry.GetAssetTypes();

        public static PakWorkerProgress PackageProgress => _worker.Progress;
        
        public static IEnumerable<IAsset> GetAssets(AssetInfo info)
            => _registry.GetAssets(info);
        
        public static bool CanCreateAssets => _projectFS != null;
        public static bool CanSave => CanCreateAssets;

        public static bool IsPackaging => _packageTask != null;
        
        public static IEnumerable<ImageAsset> Images => _registry.Images;
        
        public static void Init(IContentEditor editor)
        {
            _editor = editor;

            if (!Directory.Exists(_editor.DataDirectory))
                Directory.CreateDirectory(_editor.DataDirectory);

            _dataFS = FileSystem.FromHostDirectory(_editor.DataDirectory);
            
            LoadRecents();

            _registry.Clear();

            foreach (var assetType in ModuleManager.GetAssetTypes())
            {
                _registry.Add(assetType);
            }
            
            _editor.UpdateGoodieLists();

            _editor.UpdateMenu();

            _editor.SetCustomViewElement(null);
        }

        public static void EditProjectMetadata()
        {
            if (!CanCreateAssets)
                throw new InvalidOperationException();
            
            _editor.SelectGoodie(null);
            _editor.ShowEditor = true;

            _editor.ClearCategories();
            
            // get the type information
            var type = _metadata.GetType();
            
            // get property info for the needed properties.
            var name = type.GetProperty(nameof(_metadata.Name));
            var desc = type.GetProperty(nameof(_metadata.Description));
            var author = type.GetProperty(nameof(_metadata.Author));
            var icon = type.GetProperty(nameof(_metadata.Icon));
            var wallpaper = type.GetProperty(nameof(_metadata.Wallpaper));
            var vosBoot = type.GetProperty(nameof(_metadata.BootLogo));
            var eula = type.GetProperty(nameof(_metadata.EnableEula));
            // Category names.
            var packInfo = "Package Information";
            var images = "Images";
            var oobe = "Initial Setup";
            
            // Package information category.
            _editor.AddCategory(packInfo);
            _editor.AddCategory(images);
            _editor.AddCategory(oobe);

            // Create editors for the name, description and author.
            var nameEditor = new StringEditor();
            var descEditor = new StringEditor();
            var authEditor = new StringEditor();
            
            // Initialize them.
            nameEditor.Initialize(_metadata, name);
            descEditor.Initialize(_metadata, desc);
            authEditor.Initialize(_metadata, author);
            
            // Add them to the editor UI.
            _editor.AddEditItem(packInfo, "Package Name",
                "This is the name of your package. It's displayed in the main  menu, as the name of the custom story. It's also used as the name of the folder where save files are stored.",
                nameEditor);
            _editor.AddEditItem(packInfo, "Description",
                "Describe your custom content pack in about one or two sentences - it's displayed in the main menu.",
                descEditor);
            _editor.AddEditItem(packInfo, "Author", "Enter your name or username so people know who made this world.",
                authEditor);
            
            // Set up image editors for the pack icon, wallpaper, and boot logo.
            var iconEditor = new ImagePropertyEditor();
            var wpEditor = new ImagePropertyEditor();
            var bootEditor = new ImagePropertyEditor();

            iconEditor.Initialize(_metadata, icon);
            wpEditor.Initialize(_metadata, wallpaper);
            bootEditor.Initialize(_metadata, vosBoot);
            
            // Add them to the Images category.
            _editor.AddEditItem(images, "Package Icon", "Used in the main menu when listing your custom story.", iconEditor);
            _editor.AddEditItem(images, "vOS Desktop Wallpaper",
                "The default Desktop Wallpaper used by the game's virtual operating system (vOS). If left as nothing, we'll use the one we do for Career Mode.",
                wpEditor);
            _editor.AddEditItem(images, "vOS Boot Logo",
                "Set the logo that the vOS displays while loading the game world. If left as nothing, wwe'll use the Socially Distant logo.",
                bootEditor);
            
            // Check-boxes for the initial setup behaviour
            var eulaEditor = new BooleanEditor();
            eulaEditor.Initialize(_metadata, eula);

            _editor.AddEditItem(oobe, "Show Fake EULA Agreement",
                "If enabled, the vOS Initial Setup will show a fake License Agreement screen to the player. You can edit the EULA message by modifying the eula.txt file in the root of the project.",
                eulaEditor);
            
            // Add action for editing the eula.txt.
            _editor.AddEditAction(oobe, "Edit eula.txt",
                "Click to create/open the eula.txt file that will be used for the fake license agreement.", EditEULA);
        }
        
        public static void NewProject()
        {
            if (_editor.AskForFolder("New Project", out var folder))
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                _activeProjectFolder = folder;
                Array.Resize(ref _recents, _recents.Length + 1);

                for (var i = _recents.Length - 1; i > 0; i--)
                {
                    var r = _recents[i - 1];
                    _recents[i] = r;
                }

                _recents[0] = folder;

                SaveRecents();

                _registry.ClearAssets();
                
                _projectFS = FileSystem.FromHostDirectory(_activeProjectFolder);

                CreateAssetFolders();

                _metadata = new();

                AutoCreateAssets();
                
                SaveProject();
                
                _editor.UpdateMenu();
            }
        }
        
        public static void OpenProject(string projectFolder)
        {
            var projFS = FileSystem.FromHostDirectory(projectFolder);
            if (!projFS.FileExists("/scpack.json"))
            {
                _editor.Error("Cannot find scpack.json file - are you sure this is a Socially Distant Editor project?");
                return;
            }

            _activeProjectFolder = projectFolder;
            _projectFS = projFS;

            var meta = _projectFS.ReadAllText("/scpack.json");
            _metadata = JsonSerializer.Deserialize<ContentPackMetadata>(meta);

            _registry.ClearAssets();

            CreateAssetFolders();

            LoadAssets();
            AutoCreateAssets();
            LoadImages();
            
            _editor.UpdateMenu();
        }

        public static void SaveProject()
        {
            var metadataJson = JsonSerializer.Serialize(_metadata, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            _projectFS.WriteAllText("/scpack.json", metadataJson);

            foreach (var assetType in AssetTypes)
            {
                var path = "/Objects/" + assetType.Name;
                foreach (var asset in _registry.GetAssets(assetType))
                {
                    if (!_registry.IsDirty(asset))
                        continue;
                    
                    var aPath = path + "/" + asset.Id.ToString() + ".scasset";
                    var json = JsonSerializer.Serialize(asset, asset.GetType(), new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    
                    _projectFS.WriteAllText(aPath, json);
                }
            }
            
            _registry.ClearDirty();
            _editor.UpdateMenu();
        }

        private static void LoadRecents()
        {
            if (_dataFS.FileExists("/recents.json"))
            {
                var recents = _dataFS.ReadAllText("/recents.json");

                var recentsData = JsonSerializer.Deserialize<string[]>(recents);

                _recents = recentsData;
            }
            else
            {
                _recents = Array.Empty<string>();
            }
        }

        private static void SaveRecents()
        {
            var json = JsonSerializer.Serialize(_recents);
            _dataFS.WriteAllText("/recents.json", json);
        }

        public static void CreateAsset(AssetInfo type, string name)
        {
            if (!CanCreateAssets)
                throw new InvalidOperationException("Project not open.");
            
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Name cannot be blank.");
            
            var obj = (IAsset) Activator.CreateInstance(type.AssetType, null);

            obj.Id = Guid.NewGuid();
            obj.Name = name;

            _registry.Add(type, obj);
            _registry.SetDirty(obj);
            
            _editor.UpdateGoodies(type);
            
            SaveProject();

            SelectAsset(type, obj);
        }

        public static void SelectAsset(AssetInfo type, IAsset obj)
        {
            _editor.ExpandGoodieCategory(type);
            _editor.SelectGoodie(obj);
            _editor.ShowEditor = true;
            _editor.ClearCategories();

            var cats = new List<string>();
            foreach (var property in type.Properties.OrderBy(x=>x.Category).ThenBy(x=>x.Name))
            {
                if (!cats.Contains(property.Category))
                {
                    _editor.AddCategory(property.Category);
                    cats.Add(property.Category);
                }

                var editor = property.CreateEditor(obj);
                
                _editor.AddEditItem(property.Category, property.Name, property.Description, editor);

                editor.ValueChanged += (o, a) =>
                {
                    if (!_registry.IsDirty(obj))
                    {
                        _registry.SetDirty(obj);
                        _editor.UpdateMenu();
                    }

                    if (_registry.CheckName(obj))
                    {
                        _editor.UpdateGoodies(type);
                        _editor.ExpandGoodieCategory(type);
                        _editor.SelectGoodie(obj);
                    } 
                };
            }

            var customView = type.CreateCustomView(obj);
            if (customView != null)
            {
                _editor.SetCustomViewElement(customView.RootElement);
                customView.AssetChanged += (o, a) =>
                {
                    if (!_registry.IsDirty(obj))
                    {
                        _registry.SetDirty(obj);
                        _editor.UpdateMenu();
                    }

                    if (_registry.CheckName(obj))
                    {
                        _editor.UpdateGoodies(type);
                        _editor.ExpandGoodieCategory(type);
                        _editor.SelectGoodie(obj);
                    }
                };
            }
            else
            {
                _editor.SetCustomViewElement(null);
            }
        }
        
        private static void CreateAssetFolders()
        {
            if (!_projectFS.DirectoryExists("/Images"))
                _projectFS.CreateDirectory("/Images");
            if (!_projectFS.DirectoryExists("/Objects"))
                _projectFS.CreateDirectory("/Objects");
            if (!_projectFS.DirectoryExists("/Scripts"))
                _projectFS.CreateDirectory("/Scripts");

            foreach (var assetType in AssetTypes)
            {
                var path = "/Objects/" + assetType.Name;
                if (!_projectFS.DirectoryExists(path))
                {
                    _projectFS.CreateDirectory(path);
                }
            }
        }

        private static void LoadAssets()
        {
            foreach (var assetType in AssetTypes)
            {
                var path = "/Objects/" + assetType.Name;
                if (_projectFS.DirectoryExists(path))
                {
                    foreach (var file in _projectFS.GetFiles(path))
                    {
                        var json = _projectFS.ReadAllText(file);

                        var asset = (IAsset) JsonSerializer.Deserialize(json, assetType.AssetType);

                        _registry.Add(assetType, asset);
                    }
                }

                _editor.UpdateGoodies(assetType);
            }
        }

        private static void AutoCreateAssets()
        {
            foreach (var assetType in AssetTypes)
            {
                if (assetType.AutoCreate)
                {
                    var name = assetType.AutoCreateName;

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (_registry.GetAssets(assetType).All(x => x.Name != name))
                        {
                            CreateAsset(assetType, name);
                        }
                    }
                }
            }

            var scriptPath = "/Scripts/WorldHooks.js";
            if (!_projectFS.FileExists(scriptPath))
            {
                if (Resource.TryGetString(typeof(ContentController).Assembly,
                    "SociallyDistant.Resources.Editor.WorldHooks.js", out var res))
                {
                    _projectFS.WriteAllText(scriptPath, res);
                }
            }
        }

        public static IEnumerable<IAsset> GetAssetsOfType(Type type)
        {
            foreach (var assetType in _registry.GetAssetTypes())
            {
                foreach (var asset in _registry.GetAssets(assetType).Where(x=>x.GetType().IsAssignableTo(type)))
                    yield return asset;
            }

        }
        
        public static IEnumerable<T> GetAssetsOfType<T>() where T : IAsset
        {
            foreach (var assetType in _registry.GetAssetTypes())
            {
                foreach (var asset in _registry.GetAssets(assetType).OfType<T>())
                    yield return asset;
            }
        }

        public static void AskForImage(string title, Action<ImageAsset> callback)
        {
            _editor.ImageSelectTitle = title;
            _editor.ShowImageSelect(callback);
        }

        public static bool ImportImage(GraphicsProcessor gpu, out ImageAsset asset)
        {
            if (!_projectFS.DirectoryExists("/Images"))
                _projectFS.CreateDirectory("/Images");
            
            var fileChooser = new FileChooser();
            fileChooser.AcceptFileType("png", "PNG image");
            fileChooser.AcceptFileType("jpg", "JPEG image");
            fileChooser.AcceptFileType("jpeg", "JPEG image");
            fileChooser.AcceptFileType("bmp", "Bitmap image");
            fileChooser.Title = "Import image file";
            fileChooser.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            var result = fileChooser.Activate();
            if (result == FileOpenerResult.Ok)
            {
                var path = fileChooser.SelectedFilePath;
                

                var ext = Path.GetExtension(path);
                var guid = Guid.NewGuid().ToString() + ext;
                var assetPath = "/Images/" + guid;
                using (var iStream = File.OpenRead(path))
                {
                    using (var oStream = _projectFS.OpenFile(assetPath))
                    {
                        iStream.CopyTo(oStream);
                    }
                }

                using var aStream = _projectFS.OpenFile(assetPath);

                var texture = Texture2D.FromStream(gpu, aStream);

                var assetData = new ImageAsset(texture, assetPath);
                _registry.AddImage(assetData);

                asset = assetData;
                return true;
            }

            asset = null;
            return false;
        }
        
        private static void LoadImages()
        {
            var gpu = _editor.Graphics;

            foreach (var imagePath in _projectFS.GetFiles("/Images"))
            {
                using var stream = _projectFS.OpenFile(imagePath);
                var texture = Texture2D.FromStream(gpu, stream);
                stream.Close();
                
                var asset = new ImageAsset(texture, imagePath);
                _registry.AddImage(asset);
            }
        }

        public static void StartPackage()
        {
            if (IsPackaging)
            {
                _editor.Error("Packaging is already in progress.");
                return;
            }

            if (!CanCreateAssets)
            {
                _editor.Error("Cannot package the project - because no project is opened!");
                return;
            }

            var chooser = new FileChooser();
            chooser.Title = "Package Project";
            chooser.AcceptFileType("pak", "Thundershock Engine PAK File (Socially Distant World Pak)");
            chooser.InitialDirectory = Path.Combine(ThundershockPlatform.LocalDataPath, "packs");
            chooser.FileOpenerType = FileOpenerType.Save;
            if (!Directory.Exists(chooser.InitialDirectory))
                Directory.CreateDirectory(chooser.InitialDirectory);

            if (chooser.Activate() == FileOpenerResult.Ok)
            {
                _editor.OverlayVisibility = Visibility.Visible;
                
                var path = chooser.SelectedFilePath;
                var fs = _projectFS;
                var registry = _registry;

                var worker = new PakWorker(fs, registry, path, _metadata, _editor);

                _worker = worker;
                
                worker.Finished += WorkerOnFinished;

                _packageTask = worker.PackageAsync();
            }


        }

        private static void WorkerOnFinished(object? sender, EventArgs e)
        {
            if (_packageTask.Exception != null)
            {
                _editor.Error("An error occurred packaging the world data." + Environment.NewLine +
                              Environment.NewLine + _packageTask.Exception.ToString());
            }
            
            _packageTask = null;
            _editor.OverlayVisibility = Visibility.Collapsed;
            _worker = null;
        }

        public static void EditEULA()
        {
            if (!_projectFS.FileExists("/eula.txt"))
                _projectFS.WriteAllText("/eula.txt", "Edit the message displayed in the License Agreement screen.");

            var realPath = Path.Combine(_activeProjectFolder, "eula.txt");
            ThundershockPlatform.OpenFile(realPath);
        }
    }

}