using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SociallyDistant.Core;
using SociallyDistant.Gui.Windowing;
using Thundershock;
using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Shell.Displays
{
    public class DisplayManager : ISystem
    {
        private Scene _scene;
        private List<DisplayWindow> _displays = new();
        private FreePanel.CanvasAnchor _displayAnchor = FreePanel.CanvasAnchor.TopLeft;
        private Dictionary<Type, LauncherAttribute> _displayLauncherTypes = new();
        private Dictionary<Type, Texture2D> _iconMap = new();
        
        public FreePanel.CanvasAnchor DisplayAnchor
        {
            get => _displayAnchor;
            set
            {
                if (_displayAnchor != value)
                {
                    _displayAnchor = value;
                    ResetAnchors();
                }
            }
        }
        
        public void Init(Scene scene)
        {
            _scene = scene;
            
            foreach (var type in ThundershockPlatform.GetAllTypes<DisplayWindow>())
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                var attr = type.GetCustomAttributes(false).OfType<LauncherAttribute>().FirstOrDefault();

                if (attr == null)
                    continue;
                
                _displayLauncherTypes.Add(type, attr);

                if (!string.IsNullOrWhiteSpace(attr.IconPath))
                {
                    if (AssetManager.TryLoadTexture(attr.IconPath, out var texture))
                    {
                        _iconMap.Add(type, texture);
                    }
                }
            }

        }

        public void Unload()
        {
        }

        public void Load()
        {
        }

        public void Update(GameTime gameTime)
        {
            for (var i = 0; i < _displays.Count; i++)
            {
                var display = _displays[i];

                if (display.Window.Parent == null)
                {
                    _displays.RemoveAt(i);
                    i--;
                    continue;
                }

                display.Update(gameTime);
            }
        }

        public void Render(GameTime gameTime)
        {
        }

        private void ResetAnchors()
        {
            foreach (var display in _displays)
            {
                display.Window.ViewportAnchor = _displayAnchor;
                display.Window.ViewportAlignment = Vector2.Zero;
                display.Window.ViewportPosition = Vector2.Zero;
            }
        }

        public T OpenDisplay<T>(IRedTeamContext userContext) where T : DisplayWindow, new()
        {
            var windowManager = _scene.GetSystem<WindowManager>();

            var window = windowManager.CreateFloatingPane("Display", WindowStyle.Tile);

            var display = DisplayWindow.Create<T>(window, userContext);

            _displays.Add(display);
            
            ResetAnchors();

            return display;
        }

        public IEnumerable<Launcher> GetLaunchers()
        {
            foreach (var type in _displayLauncherTypes.Keys)
            {
                var attr = _displayLauncherTypes[type];
                var icon = null as Texture2D;
                if (_iconMap.ContainsKey(type))
                {
                    icon = _iconMap[type];
                }

                yield return new Launcher(this, attr, icon, type);
            }
        }

        private void OpenDisplay(IRedTeamContext ctx, Type winType)
        {
            // TODO: Switching to opened single-instance displays.
            
            var windowManager = _scene.GetSystem<WindowManager>();

            var window = windowManager.CreateFloatingPane("Display", WindowStyle.Tile);

            var display = DisplayWindow.Create(window, ctx, winType);

            _displays.Add(display);
            
            ResetAnchors();

        }
        
        public class Launcher
        {
            private DisplayManager _dm;
            private Texture2D _icon;
            private LauncherAttribute _launcher;
            private Type _win;

            public Texture2D Icon => _icon;
            public string Name => _launcher.DefaultTitle;
            
            public Launcher(DisplayManager displayManager, LauncherAttribute launcher, Texture2D icon, Type win)
            {
                _dm = displayManager;
                _launcher = launcher;
                _win = win;
                _icon = icon;
            }

            public void Open(IRedTeamContext ctx)
            {
                _dm.OpenDisplay(ctx, _win);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LauncherAttribute : Attribute
    {
        public string DefaultTitle { get; }
        public string IconPath { get; }

        public LauncherAttribute(string title, string iconPath)
        {
            DefaultTitle = DefaultTitle;
            IconPath = iconPath;
        }
    }
}