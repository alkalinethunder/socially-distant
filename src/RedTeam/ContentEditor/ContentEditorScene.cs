using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text.Json;
using Microsoft.Xna.Framework;
using RedTeam.Components;
using RedTeam.Core;
using RedTeam.Core.Components;
using RedTeam.Core.Config;
using RedTeam.Core.ContentEditors;
using RedTeam.Core.Gui.Elements;
using RedTeam.Gui.Elements;
using Thundershock;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Input;
using Thundershock.Rendering;

namespace RedTeam.ContentEditor
{
    public class ContentEditorScene : Scene
    {
        private GuiSystem _gui;
        private WindowManager _wm;
        private ContentManager _content;
        private ContentPack _currentPack;
        private List<Editor> _editors = new();
        private Editor _currentEditor;
        
        #region GUI Elements
        
        private Stacker _master = new();
        private Stacker _slave = new();
        private Stacker _inner = new();
        private Stacker _editorStacker = new();

        private ConsoleControl _thundershockConsole = new();

        private Stacker _dbStacker = new();
        private StringList _dbList = new();
        private Button _newPackButton = new();
        private TextEntryDialog _newDialog;
        private StringList _typeList = new();
        private Pane _dbs;
        private Pane _contentTypes;
        private Pane _editor;
        private Pane _consolePanel;
        
        #endregion
        
        protected override void OnLoad()
        {
            Camera = new Camera2D();

            _content = App.GetComponent<ContentManager>();
            
            // Disable post-process effects.
            Game.PostProcessSettings.EnableBloom = false;
            Game.PostProcessSettings.EnableShadowMask = false;

            _gui = AddComponent<GuiSystem>();
            _wm = AddComponent<WindowManager>();

            _dbs = _wm.CreatePane("Content Packs");
            _contentTypes = _wm.CreatePane("Data");
            _editor = _wm.CreatePane("Editor");
            _consolePanel = _wm.CreatePane("Console");

            _slave.Direction = StackDirection.Horizontal;
            _editorStacker.Direction = StackDirection.Horizontal;
            
            _slave.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _editor.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _inner.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            _editorStacker.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);
            
            _gui.AddToViewport(_master);

            _master.Children.Add(_slave);

            _slave.Children.Add(_dbs);
            _slave.Children.Add(_inner);

            _inner.Children.Add(_editorStacker);
            _inner.Children.Add(_consolePanel);

            _editorStacker.Children.Add(_contentTypes);
            _editorStacker.Children.Add(_editor);
            
            _consolePanel.FixedHeight = 300;

            _dbs.FixedWidth = 200;
            _contentTypes.FixedWidth = 200;

            _consolePanel.Content.Add(_thundershockConsole);

            LoadDefaultConsolePalette();
            
            // set up thundershock's console output
            SetupDebugLog();

            _dbs.Content.Add(_dbStacker);

            _dbStacker.Children.Add(_newPackButton);

            var newText = new TextBlock();
            newText.Text = "CREATE NEW";
            _newPackButton.Children.Add(newText);

            _dbStacker.Children.Add(_dbList);
            _dbList.Properties.SetValue(Stacker.FillProperty, StackFill.Fill);

            _newPackButton.MouseUp += HandleNewPackButton;

            ListPacks();
            
            _dbList.SelectedIndexChanged += DbListSelectedItemChanged;
            
            _contentTypes.Content.Add(_typeList);

            LoadEditors();   
            
            _typeList.SelectedIndexChanged += TypeListOnSelectedIndexChanged;
            
            base.OnLoad();
        }

        private void TypeListOnSelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_currentEditor != null)
            {
                _currentEditor.Unbind();
                _currentEditor = null;
            }

            if (_typeList.SelectedIndex >= 0)
            {
                _currentEditor = _editors.First(x => x.Name == _typeList.SelectedItem);

                _currentEditor.Bind(_editor, _currentPack);
            }
        }

        private void LoadEditors()
        {
            foreach (var type in ThundershockPlatform.GetAllTypes<Editor>())
            {
                if (type.GetCustomAttributes(false).OfType<CustomEditorAttribute>().Any())
                {
                    var editor = (Editor) Activator.CreateInstance(type, null);

                    _editors.Add(editor);

                    _typeList.AddItem(editor.Name);
                }
            }

            foreach (var type in ThundershockPlatform.GetAllTypes<IContentData>())
            {
                var attr = type.GetCustomAttributes(false).OfType<EditableDataAttribute>().FirstOrDefault();

                if (attr != null)
                {
                    var editorType = typeof(MultiDataEditor<>).MakeGenericType(new[] {type});

                    var createMethod =
                        Expression.Call(editorType, "Create", new[] {type}, Expression.Constant(attr.Name));

                    var variable = Expression.Variable(typeof(Editor), "ins");

                    var cast = Expression.Convert(createMethod, typeof(Editor));

                    var assignment = Expression.Assign(variable, cast);

                    var body = Expression.Block(typeof(Editor), new[] {variable}, assignment, variable);

                    var lambda = Expression.Lambda<Func<Editor>>(body);

                    var creator = lambda.Compile();

                    var editor = creator();
                    
                    _editors.Add(editor);
                    _typeList.AddItem(attr.Name);
                }
            }
        }
        
        private void DbListSelectedItemChanged(object? sender, EventArgs e)
        {
            if (_dbList.SelectedIndex >= 0)
                SelectContentPack(_dbList.SelectedItem);
        }

        private void ListPacks()
        {
            _dbList.Clear();

            foreach (var pack in _content.Packs)
            {
                _dbList.AddItem(pack.Id);
            }
        }
        
        private void HandleNewPackButton(object? sender, MouseButtonEventArgs e)
        {
            if (_newDialog == null)
            {
                _newDialog = _wm.MakeTextPrompt("New Content Pack", "Please enter a name for your new RED TEAM Content Pack file.");
                
                _newDialog.TextEntered += CreateNewContentPack;
                _newDialog.Cancelled += HandleNewDialogCancelled;
            }
        }

        private void HandleNewDialogCancelled(object? sender, EventArgs e)
        {
            _newDialog = null;
        }

        private void CreateNewContentPack(string obj)
        {
            _content.CreatePack(obj);
            ListPacks();
            SelectContentPack(obj);
        }

        private void SelectContentPack(string packId)
        {
            _dbList.SelectedIndex = _dbList.IndexOf(packId);
            _currentPack = _content.Packs.FirstOrDefault(x => x.Id == packId);
        }

        private void SetupDebugLog()
        {
            App.GetComponent<EditorConsole>().SetConsole(_thundershockConsole);
        }

        private void LoadDefaultConsolePalette()
        {
            var asm = typeof(IRedTeamContext).Assembly;
            var res = asm.GetManifestResourceStream("RedTeam.Core.Resources.RedTermPalettes.default.json");
            using var reader = new StreamReader(res);

            var json = reader.ReadToEnd();

            var obj = JsonSerializer.Deserialize<RedTermPalette>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });

            _thundershockConsole.ColorPalette = obj.ToColorPalette();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            base.OnUpdate(gameTime);

            _contentTypes.Visibility = _currentPack != null ? Visibility.Visible : Visibility.Hidden;
            _editor.Visibility = _currentEditor != null ? Visibility.Visible : Visibility.Hidden;
        }
    }
}