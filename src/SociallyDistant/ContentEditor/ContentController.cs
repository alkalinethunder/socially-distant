using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Silk.NET.OpenGL;
using SociallyDistant.Core;
using SociallyDistant.Core.ContentEditors;
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
        public static IEnumerable<string> RecentProjects => _recents;

        public static IEnumerable<AssetInfo> AssetTypes => _registry.GetAssetTypes();

        public static IEnumerable<IAsset> GetAssets(AssetInfo info)
            => _registry.GetAssets(info);
        
        public static bool CanCreateAssets => _projectFS != null;
        public static bool CanSave => CanCreateAssets && _registry.HasDirty;
        
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

                CreateAssetFolders();
                
                _projectFS = FileSystem.FromHostDirectory(_activeProjectFolder);
                
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
                var path = "/" + assetType.Name;
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
            foreach (var assetType in AssetTypes)
            {
                var path = "/" + assetType.Name;
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
                var path = "/" + assetType.Name;
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
        }

        public static IEnumerable<T> GetAssetsOfType<T>() where T : IAsset
        {
            foreach (var assetType in _registry.GetAssetTypes())
            {
                foreach (var asset in _registry.GetAssets(assetType).OfType<T>())
                    yield return asset;
            }
        }
    }
}