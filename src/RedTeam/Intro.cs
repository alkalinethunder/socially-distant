using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Core.Config;
using Thundershock.Components;
using Thundershock.Input;
using Thundershock;
using Thundershock.Rendering;

namespace RedTeam
{
    public class Intro : Scene
    {
        private bool _hasInitialized = false;
        private double _broadcasterDelay;
        private SoundEffect _welcomee;
        private SoundEffect _typingSound;
        private double _promptWait;
        private double _promptTime;
        private VorbisPlayer _glitchPlayer;
        private bool _animDone;
        private SoundEffectInstance _typingSoundInstance;
        private double _typeWait = 1;
        private int _animState = 0;
        private const double _blinkTime = 0.1;
        private double _blink;
        private int _typePos;
        private string _type;
        private bool _blinkOn = true;
        private Vector2 _typeMeasure;
        private Vector2 _typeLocation;        
        private TextComponent _cursorText;
        private bool _glitching = false;
        private TextComponent _poweredBy;
        private TextComponent _thundershock;
        private TextComponent _michael;
        private TextComponent _presents;
        private TextComponent _redTeam;
        private TextComponent _prompt;
        private Backdrop _backdrop;
        private SolidRectangle _cursor;
        
        protected override void OnLoad()
        {
            Camera = new Camera2D();

            if (App.GetComponent<RedConfigManager>().ActiveConfig.BroadcasterMode)
                _broadcasterDelay = 7.5;
            else
                OnInit();
            
            _backdrop = AddComponent<Backdrop>();
            
            _backdrop.Texture = Game.Content.Load<Texture2D>("Backgrounds/DesktopBackgroundImage2");

            base.OnLoad();
        }

        private void OnInit()
        {
            if (_hasInitialized)
                return;
            
            _hasInitialized = true;
            
            _cursor = AddComponent<SolidRectangle>();
            _glitchPlayer = AddComponent<VorbisPlayer>();

            
            _welcomee = Game.Content.Load<SoundEffect>("Sounds/Intro/Welcome");
            _typingSound = Game.Content.Load<SoundEffect>("Sounds/Intro/Typing");
            _typingSoundInstance = _typingSound.CreateInstance();

            _typingSoundInstance.IsLooped = true;
            
            var big = Game.Content.Load<SpriteFont>("Fonts/Intro/Big");
            var small = Game.Content.Load<SpriteFont>("Fonts/Intro/Small");

            _prompt = AddComponent<TextComponent>();
            _redTeam = AddComponent<TextComponent>();
            _presents = AddComponent<TextComponent>();
            _michael = AddComponent<TextComponent>();
            _poweredBy = AddComponent<TextComponent>();
            _thundershock = AddComponent<TextComponent>();

            _redTeam.Font = big;
            _michael.Font = big;
            _thundershock.Font = big;

            _prompt.Font = small;
            _poweredBy.Font = small;
            _presents.Font = small;

            _prompt.Text = "press any key to continue . . .";
            _poweredBy.Text = "powered by";
            _presents.Text = "presents";

            _thundershock.Text = "thundershock engine";
            _redTeam.Text = "RED TEAM";
            _michael.Text = "Michael VanOverbeek";

            _redTeam.Color = ThundershockPlatform.HtmlColor("#f71b1b");

            _michael.Color = ThundershockPlatform.HtmlColor("#1baaf7");
            _thundershock.Color = Color.Green;

            _prompt.Visible = false;
            _thundershock.Visible = false;
            _redTeam.Visible = false;
            _presents.Visible = false;
            _poweredBy.Visible = false;
            
            _presents.Visible = true;

            _prompt.Origin = new Vector2(0.5f, 0.75f);
            
            var pos = _michael.Position;
            var size = _michael.TextMeasure;

            pos.X -= size.X / 2;
            pos.Y += size.Y / 2;
            
            _presents.Position = pos;
            _presents.Pivpt = new Vector2(0, 0.5f);
            _presents.Visible = false;
            
            StartGlitchEffect("RedTeam.Resources.Audio.MichaelGlitch.ogg");

            pos = _thundershock.Position;
            size = _thundershock.TextMeasure;

            pos.X -= size.X * _thundershock.Pivpt.X;
            pos.Y -= size.Y * _thundershock.Pivpt.Y;

            _poweredBy.Position = pos;
            _poweredBy.Pivpt = new Vector2(0, 0.5f);

            var input = App.GetComponent<InputManager>();
            input.KeyDown += HandleKeyDown;
            

        }

        protected override void OnUnload()
        {
            var input = App.GetComponent<InputManager>();
            input.KeyDown -= HandleKeyDown;

            _typingSoundInstance?.Dispose();

            Game.PostProcessSettings.EnableGlitch = false;
            Game.PostProcessSettings.GlitchIntensity = 0;
            Game.PostProcessSettings.GlitchSkew = 0;
            
            base.OnUnload();
        }

        private void HandleKeyDown(object? sender, KeyEventArgs e)
        {
            if (_animDone)
            {
                StartGlitchEffect("RedTeam.Resources.Audio.MichaelGlitch.ogg");
                _animState++;
            }
            else
            {
                Done();
            }
        }

        private void StartGlitchEffect(string resource)
        {
            _glitchPlayer.OpenResource(this.GetType().Assembly, resource);
            _glitching = true;
        }
        
        private void Done()
        {
            App.LoadScene<RedTeamHackerScene>();
        }

        private void StartTyping(TextComponent text)
        {
            _typingSoundInstance.Play();
            _cursorText = text;
            _typeMeasure = text.TextMeasure;
            _typeLocation = text.Position;
            _typePos = 0;
            _type = _cursorText.Text;
            _cursorText.Text = string.Empty;
        }

        private void StopTyping()
        {
            _cursorText = null;
            _type = null;
            _typePos = 0;
            _typingSoundInstance.Stop();
        }
        
        protected override void OnUpdate(GameTime gameTime)
        {
            if (_broadcasterDelay > 0)
            {
                _broadcasterDelay -= gameTime.ElapsedGameTime.TotalSeconds;
                return;
            }

            if (!_hasInitialized)
                OnInit();
            
            if (_typeWait >= 0 && _cursorText == null)
                _typeWait -= gameTime.ElapsedGameTime.TotalSeconds;
            
            if (_glitching)
            {
                var pow = _glitchPlayer.Power;

                Game.PostProcessSettings.EnableGlitch = true;
                Game.PostProcessSettings.GlitchIntensity = pow;
                Game.PostProcessSettings.GlitchSkew = pow / 8;
                
                if (!_glitchPlayer.IsPlaying)
                {
                    _glitching = false;
                }
            }
            else
            {
                Game.PostProcessSettings.EnableGlitch = false;
                Game.PostProcessSettings.GlitchIntensity = 0;
                Game.PostProcessSettings.GlitchSkew = 0;
            }

            _blink += gameTime.ElapsedGameTime.TotalSeconds;
            if (_blink >= _blinkTime)
            {
                _blinkOn = !_blinkOn;
                _blink = 0;
                _typePos++;
                if (_type != null && _typePos > _type.Length)
                {
                    StopTyping();
                }
            }
            
            if (_cursorText != null)
            {
                _cursorText.Text = _type.Substring(0, _typePos);
                _cursorText.Position = _typeLocation - _typeMeasure * _cursorText.Pivpt;
                _cursorText.Position += _cursorText.TextMeasure * _cursorText.Pivpt;
                var m = _cursorText.Font.MeasureString("#");
                
                _cursor.Visible = _blinkOn;
                _cursor.Color = _cursorText.Color;

                _cursor.Size = m;

                _cursor.Origin = _cursorText.Origin;
                _cursor.Pivot = _cursorText.Pivpt;

                var pos = _cursorText.Position;

                pos.X += _cursorText.TextMeasure.X / 2;
                pos.X += m.X / 2;
                
                _cursor.Position = pos;
            }
            else
            {
                _cursor.Visible = (_typeWait > 0);
            }

            switch (_animState)
            {
                case 0:
                    if (!_glitching)
                    {
                        _presents.Visible = true;
                        StartTyping(_presents);
                        _animState++;
                        _typeWait = 1;
                    }
                    break;
                case 1:
                    if (_cursorText == null)
                    {
                        if (_typeWait <= 0)
                        {
                            _poweredBy.Visible = false;
                            _thundershock.Visible = false;
                            StartGlitchEffect("RedTeam.Resources.Audio.ThundershockGlitch.ogg");
                            _animState++;
                        }
                    }
                    break;
                case 2:
                    if (!_glitching)
                    {
                        _michael.Visible = false;
                        _presents.Visible = false;
                        _animState++;
                        _poweredBy.Visible = true;
                        StartTyping(_poweredBy);
                    }
                    break;
                case 3:
                    if (_cursorText == null)
                    {
                        _typeWait = 1;
                        _thundershock.Visible = true;
                        StartTyping(_thundershock);
                        _animState++;
                    }
                    break;
                case 4:
                    if (_cursorText == null)
                    {
                        if (_typeWait <= 0)
                        {
                            StartGlitchEffect("RedTeam.Resources.Audio.RedTeamGlitch.ogg");
                            _animState++;
                        }
                    }
                    break;
                case 5:
                    if (!_glitching)
                    {
                        _poweredBy.Visible = false;
                        _thundershock.Visible = false;
                        _redTeam.Visible = true;
                        StartTyping(_redTeam);
                        _animState++;
                    }
                    break;
                case 6:
                    if (_cursorText == null)
                    {
                        _welcomee.Play();
                        _animState++;
                        _prompt.Visible = true;
                        _promptWait = _welcomee.Duration.TotalSeconds;
                        _promptTime = 0;
                    }

                    break;
                case 7:
                    var percent = (float) (_promptTime / _promptWait);
                    var alpha = MathHelper.Clamp(percent, 0, 1);
                    _prompt.Color = new Color(_prompt.Color, alpha);
                    _promptTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_promptTime >= _promptWait)
                    {
                        _animState++;
                        _animDone = true;
                    }
                    break;
                case 9:
                    if (!_glitching)
                    {
                        Done();
                    }

                    break;
            }
        }
    }
}