using System;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using SociallyDistant.Core.Config;
using SociallyDistant.Core.Game;
using SociallyDistant.Core.Gui.Elements;
using SociallyDistant.Core.SaveData;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Audio;
using Thundershock.Components;
using Thundershock.Core;
using Thundershock.Core.Debugging;
using Thundershock.Core.Rendering;
using Thundershock.Core.Scripting;
using Thundershock.Gui;
using Thundershock.Gui.Elements;
using Thundershock.Gui.Elements.Console;
using Thundershock.Rendering;

namespace SociallyDistant
{
    public class BootScreen : Scene
    {
        #region GLOBAL REFERENCES

        private SaveManager _saveManager;
        private RedConfigManager _redConf;

        #endregion

        #region Script

        private ScriptEngine _worldScript = null;

        #endregion
        
        #region COMPONENTS

        private Transform2D _logoTransform = new();
        private Sprite _logoSprite = new();
        private Transform2D _backdropTransform = new();
        private Sprite _backdrop = new();
        
        #endregion
        
        #region SYSTEMS

        private WindowManager _winManager;

        #endregion

        #region UI

        private Oobe _oobe;
        private Throbber _bootThrobber = new();

        #endregion

        #region State

        private int _bootState = 0;
        private bool _bootSet = false;
        private float _transition = 0;
        private Task _simulationPreload;
        
        #endregion
        
        protected override void OnLoad()
        {
            // Stop playing music.
            MusicPlayer.Stop();
            
            // Grab app references.
            _saveManager = Game.GetComponent<SaveManager>();
            _redConf = Game.GetComponent<RedConfigManager>();
            
            // Add the gui system to the scene.
            var bg = SpawnObject();
            var logo = SpawnObject();

            // Add components to these entities.
            bg.AddComponent(_backdropTransform);
            bg.AddComponent(_backdrop);
            logo.AddComponent(_logoTransform);
            logo.AddComponent(_logoSprite);
            
            // Window manager.
            _winManager = RegisterSystem<WindowManager>();
            
            // Set the controls for the heart of the Negati---I mean...uhhh...
            // Set the boot screen logo texture. Sorry. Had LBP on the brain.
            _logoSprite.Texture = _saveManager.ContentPack.BootLogo;
            _backdrop.Texture = _saveManager.ContentPack.Backdrop;
            
            // Set the width/height of the logo based on aspect ratio.
            var desiredHeight = 260f;
            var aspect = (float) _logoSprite.Texture.Width / (float) _logoSprite.Texture.Height;
            var desiredWidth = desiredHeight * aspect;
            _logoSprite.Size = new Vector2(desiredWidth, desiredHeight);
            
            // Set the size of the backdrop so it fills the entire screen.
            desiredHeight = PrimaryCameraSettings.OrthoHeight;
            var scale = desiredHeight / _backdrop.Texture.Height;
            desiredWidth = _backdrop.Texture.Width * scale;
            _backdrop.Size = new Vector2(desiredWidth, desiredHeight);

            Gui.AddToViewport(_bootThrobber);

            _bootThrobber.Properties.SetValue(FreePanel.AnchorProperty, new FreePanel.CanvasAnchor(0.5f, 0.75f, 0, 0));
            _bootThrobber.ThrobberSize = 48;
            
            base.OnLoad();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            // Stop us from fucking up the preloader.
            if (_saveManager.IsPreloading)
                return;

            if (_saveManager.PreloadException != null)
            {
                GoToScene<MainMenu>();
                return;
            }

            if (_worldScript == null)
                InitWorldScript();

            switch (_bootState)
            {
                case 0:
                    _backdrop.Color = Color.Transparent;
                    _logoSprite.Color = Color.Transparent;
                    _bootThrobber.Opacity = 0;

                    _transition += (float) gameTime.ElapsedGameTime.TotalSeconds;
                    
                    if (_transition >= 2)
                    {
                        _transition = 0;
                        
                        if (_worldScript.IsDefined("onCreate") && _saveManager.CurrentGame.IsNewGame)
                        {
                            _worldScript.CallIfDefined("onCreate", _saveManager.CurrentGame);
                            _saveManager.CurrentGame.IsNewGame = false;
                            _saveManager.Save();
                        }
                        
                        _bootState++;
                    }
                    
                    break;
                case 1:
                    _transition += (float) gameTime.ElapsedGameTime.TotalSeconds * 2;
                    _backdrop.Color = Color.White * _transition;
                    if (_transition >= 1)
                    {
                        _backdrop.Color = Color.White;
                        _transition = 0;
                        _bootState++;
                    }
                    break;
                case 2:
                    if (_saveManager.CurrentGame.HasPlayer)
                    {
                        _bootState += 2;
                    }
                    else
                    {
                        CreateOobe();
                        _bootState++;
                    }
                    break;
                case 3:
                    _oobe.Update((float) gameTime.ElapsedGameTime.TotalSeconds);
                    
                    if (_saveManager.CurrentGame.HasPlayer)
                    {
                        _oobe = null;
                        _bootState++;
                    }
                    break;
                case 4:
                    _transition += (float) gameTime.ElapsedGameTime.TotalSeconds * 2;
                    
                    _logoSprite.Color = Color.White * _transition;
                    _bootThrobber.Opacity = _transition;

                    if (_transition >= 1)
                    {
                        _logoSprite.Color = Color.White;
                        _bootThrobber.Opacity = 1;
                        _bootState++;
                        _transition = 0;
                        _simulationPreload = Simulation.BeginPreload(_saveManager);
                    }
                    break;
                case 5:
                    if (_simulationPreload.Exception != null)
                    {
                        var ex = _simulationPreload.Exception;

                        Logger.GetLogger().Log("Simulation preload has failed. Game will return to main menu now.",
                            LogLevel.Error);
                        Logger.GetLogger().LogException(ex);

                        GoToScene<MainMenu>();
                    }
                    
                    if (_simulationPreload.IsCompleted)
                    {
                        Logger.GetLogger().Log("Simulation preload has finished. WE. ARE. READY. TO. ROLLLLLLLLLLL!",
                            LogLevel.Message);
                        _simulationPreload = null;
                        _bootState++;
                    }

                    break;
                case 6:
                    _transition += (float)gameTime.ElapsedGameTime.TotalSeconds / 2;
                    if (_transition >= 1)
                    {
                        _bootState++;
                        _transition = 0;
                    }

                    break;
                case 7:
                    _transition += (float) gameTime.ElapsedGameTime.TotalSeconds * 2;
                    
                    _logoSprite.Color = Color.White * (1 -_transition);
                    _bootThrobber.Opacity = 1 - _transition;

                    if (_transition >= 1)
                    {
                        _logoSprite.Color = Color.Transparent;
                        _bootThrobber.Opacity = 0;
                        _bootState++;
                        _transition = 0;
                        GoToScene<Workspace>();
                    }

                    break;
            }
            
            base.OnUpdate(gameTime);
        }

        private void CreateOobe()
        {
            _oobe = new Oobe(this);
        }
        
        private void InitWorldScript()
        {
            _worldScript = new();

            using var stream = _saveManager.OpenWorldScript();

            _worldScript.ExecuteStream(stream);

            stream.Close();
        }
    }
}