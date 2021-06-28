using System.Numerics;
using Thundershock;
using Thundershock.Audio;
using Thundershock.Components;
using Thundershock.Core;
using Thundershock.Core.Input;
using Thundershock.Rendering;

namespace RedTeam
{
    public class Intro : Scene
    {
        private Transform _cameraTransform = new();
        private CameraComponent _cameraSettings = new();

        private Transform2D _thundershockTransform = new();
        private TextComponent _thundershock = new();

        private TextComponent _powerMeter = new();
        private Transform2D _powerTransform = new();
        
        private Song _song;
        
        private string _targetText = string.Empty;
        private int _cursorPos = 0;
        private int _state;
        private double _typeDelay = 0.05;
        private double _typeStartDelay = 2;
        private double _typeEndDelay = 3;
        
        protected override void OnLoad()
        {
            _powerTransform.Position.Y = -200;

            var powerEntry = SpawnObject();
            powerEntry.AddComponent(_powerTransform);
            powerEntry.AddComponent(_powerMeter);
            
            var camera = SpawnObject();
            camera.Name = "Scene Camera";
            
            camera.AddComponent(_cameraTransform);
            camera.AddComponent(_cameraSettings);

            _cameraSettings.ProjectionType = CameraProjectionType.Orthographic;

            var tsEntity = SpawnObject();

            tsEntity.AddComponent(_thundershockTransform);
            tsEntity.AddComponent(_thundershock);
            
            _thundershock.Text = string.Empty;
            _targetText = "Thundershock Engine";
            
            InputSystem.KeyUp += InputSystemOnKeyUp;
        }

        private void InputSystemOnKeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Keys.Enter)
            {
                GoToScene<MainMenu>();
            }
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _powerMeter.Text = MusicPlayer.Power.ToString();
            
            if (_state == 0)
            {
                _typeStartDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_typeStartDelay <= 0)
                {
                    _state++;
                }
            }
            else if (_state == 1)
            {
                _typeDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_typeDelay <= 0)
                {
                    _typeDelay = 0.05;
                    _cursorPos++;

                    _thundershock.Text = _targetText.Substring(0, _cursorPos);
                    
                    if (_cursorPos >= _targetText.Length)
                    {
                        _state++;
                    }
                }
            }
            else if (_state == 2)
            {
                _typeEndDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                if (_typeEndDelay <= 0)
                {
                    _thundershock.Color = Color.Red;
                    _state++;
                }
            }
            
            base.OnUpdate(gameTime);
        }
        
    }
}