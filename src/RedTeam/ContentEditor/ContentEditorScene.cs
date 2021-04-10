using System.Collections;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using RedTeam.Components;
using RedTeam.Config;
using RedTeam.Gui.Elements;
using Thundershock;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Rendering;

namespace RedTeam.ContentEditor
{
    public class ContentEditorScene : Scene
    {
        private GuiSystem _gui;
        private WindowManager _wm;

        #region GUI Elements

        private Stacker _master = new();
        private Stacker _slave = new();
        private Stacker _inner = new();
        private Stacker _editorStacker = new();

        private ConsoleControl _thundershockConsole = new();

        private StringList _dbSelector = new();

        private CheckBox _checkTest = new();
        private TextBlock _checkLabel = new();
        
        private Pane _dbs;
        private Pane _contentTypes;
        private Pane _editor;
        private Pane _consolePanel;
        
        #endregion
        
        protected override void OnLoad()
        {
            Camera = new Camera2D();

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

            _dbs.Content.Add(_dbSelector);

            for (var i = 0; i < 10; i++)
            {
                _dbSelector.AddItem($"Item {i + 1}");
            }

            _editor.Content.Add(_checkTest);
            _checkTest.Children.Add(_checkLabel);
            
            base.OnLoad();
        }

        private void SetupDebugLog()
        {
            App.GetComponent<EditorConsole>().SetConsole(_thundershockConsole);
        }

        private void LoadDefaultConsolePalette()
        {
            var asm = this.GetType().Assembly;
            var res = asm.GetManifestResourceStream("RedTeam.Resources.RedTermPalettes.default.json");
            using var reader = new StreamReader(res);

            var json = reader.ReadToEnd();

            var obj = JsonSerializer.Deserialize<RedTermPalette>(json, new JsonSerializerOptions
            {
                IncludeFields = true
            });

            _thundershockConsole.ColorPalette = obj.ToColorPalette();
        }
    }
}