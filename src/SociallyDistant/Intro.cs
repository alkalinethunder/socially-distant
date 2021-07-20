using System.Numerics;
using Thundershock;
using Thundershock.Audio;
using Thundershock.Components;
using Thundershock.Core;
using Thundershock.Core.Audio;
using Thundershock.Core.Rendering;
using Thundershock.Rendering;

namespace SociallyDistant
{
    public class Intro : Scene
    {
        #region Resources

        private Song _mmTheme;
        private Sound _typeSound;
        private Font _tsFont;
        private Font _pbFont;
        private Font _atFont;
        private Song _introBg;
        private Texture2D _atCircle;
        private string _at = "Alkaline Thunder";
        private string _ts = "Thundershock Engine";
        private string _pb = "Powered by";
        private Color _atColor = ThundershockPlatform.HtmlColor("#0376bd");

        #endregion

        #region State

        private int _state;
        private float _atFade;
        private double _atTime;
        private TextComponent _typer;
        private double _typeLength;
        private double _typeTime;
        private string _textToType;
        
        #endregion

        #region Audio

        private Sound _glitch1;

        #endregion

        #region Components

        private Transform2D _poweredByTransform = new();
        private Transform2D _tsTransform = new();
        private TextComponent _poweredBy = new TextComponent();
        private TextComponent _tsText = new();
        private TextComponent _atText = new();
        private Sprite _atLogoSprite = new();
        private Transform2D _atSpriteTransform = new();
        private Transform2D _atTextTransform = new();

        
        
        #endregion

        protected override void OnLoad()
        {
            // This scene works a lot better in perspective mode.
            PrimaryCameraSettings.ProjectionType = CameraProjectionType.Perspective;
            
            _introBg = Song.FromOggResource(GetType().Assembly, "SociallyDistant.Resources.Bgm.Intro.ogg");
            _glitch1 = Sound.FromOggResource(GetType().Assembly, "SociallyDistant.Resources.Audio.ThundershockGlitch.ogg");
            _typeSound = Sound.FromOggResource(GetType().Assembly, "SociallyDistant.Resources.Audio.Typing.ogg");
            _mmTheme = Song.FromOggResource(GetType().Assembly, "SociallyDistant.Resources.Bgm.Menu.ogg");
            
            MusicPlayer.PlaySong(_introBg);
            
            _atFont = Font.FromResource(Game.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Fonts.AlkalineThunder.ttf");
            _atFont.Size = 38;

            _atCircle = Texture2D.FromResource(Game.Graphics, GetType().Assembly,
                "SociallyDistant.Resources.Brand.atcomputercircle.png");

            var atTextEntity = SpawnObject();
            var atSpriteEntity = SpawnObject();
            var tsEntity = SpawnObject();
            var pbEntity = SpawnObject();
            
            tsEntity.AddComponent(_tsTransform);
            pbEntity.AddComponent(_poweredByTransform);
            
            tsEntity.AddComponent(_tsText);
            pbEntity.AddComponent(_poweredBy);
            
            atTextEntity.AddComponent(_atTextTransform);
            atTextEntity.AddComponent(_atText);
            
            atSpriteEntity.AddComponent(_atSpriteTransform);
            atSpriteEntity.AddComponent(_atLogoSprite);

            _atLogoSprite.Texture = _atCircle;
            _atText.Font = _atFont;

            _atText.Text = _at;

            _atText.Font.Size = (int)(_atLogoSprite.Size.Y - (_atLogoSprite.Size.Y / 4));

            _atTextTransform.Position.X += (_atLogoSprite.Size.X / 2) + 3;
            
            PrimaryCamera.GetComponent<Transform>().Scale = Vector3.One * 312;

            var measure = _atFont.MeasureString(_at).X;
            _atSpriteTransform.Position.X -= (measure / 2) + 3;

            _poweredBy.Text = _pb;
            _tsText.Text = _ts;

            _tsFont = Font.FromResource(Game.Graphics, GetType().Assembly, "SociallyDistant.Resources.Fonts.Console.Bold.ttf");
            _pbFont = Font.FromResource(Game.Graphics, GetType().Assembly, "SociallyDistant.Resources.Fonts.Console.Regular.ttf");

            _tsText.Font = _tsFont;
            _poweredBy.Font = _pbFont;

            _tsFont.Size = 20;
            _pbFont.Size = 12;

            var tsMeasure = _tsFont.MeasureString(_ts);
            
            _poweredByTransform.Position.Y -= (_pbFont.LineHeight / 2f);
            _poweredByTransform.Position.Y -= (tsMeasure.Y / 2);
            
            _tsTransform.Position.Y += _pbFont.LineHeight / 2f;
            _tsText.Pivot = new Vector2(0, 0.5f);
            _poweredBy.Pivot = _tsText.Pivot;
            _tsTransform.Position.X -= tsMeasure.X / 2;
            _poweredByTransform.Position.X = _tsTransform.Position.X;

            base.OnLoad();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            switch (_state)
            {
                case 0:
                    _tsText.Color = Color.Transparent;
                    _poweredBy.Color = Color.Transparent;
                    
                    PrimaryCameraSettings.SkyColor = Color.Black;
                    
                    _atText.Color = Color.Transparent;
                    _atLogoSprite.Color = Color.Transparent;
                    
                    _atTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_atTime >= 1)
                    {
                        _state++;
                        _atTime = 0;
                    }
                    break;
                case 1:
                    _atText.Color = Color.Transparent;
                    _atLogoSprite.Color = Color.White * _atFade;

                    _atFade = MathHelper.Clamp(_atFade + (float) gameTime.ElapsedGameTime.TotalSeconds * 4, 0, 1);
                    if (_atFade >= 1)
                    {
                        _atFade = 0;
                        _state++;
                        _atLogoSprite.Color = Color.White;
                    }
                    
                    break;
                case 2:
                    _atTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_atTime >= 1)
                    {
                        _atTime = 0;
                        _state++;
                    }

                    break;
                case 3:
                    PrimaryCameraSettings.SkyColor = Color.Lerp(Color.White, _atColor, _atFade);
                    _atText.Color = Color.White * _atFade;
                    
                    _atFade = MathHelper.Clamp(_atFade + (float) gameTime.ElapsedGameTime.TotalSeconds * 2, 0, 1);
                    if (_atFade >= 1)
                    {
                        _atFade = 0;
                        _state++;
                        _atText.Color = Color.White;
                        PrimaryCameraSettings.SkyColor = _atColor;
                    }

                    break;
                case 4:
                    var scale = PrimaryCamera.GetComponent<Transform>().Scale;
                    scale.X -= (float) gameTime.ElapsedGameTime.TotalSeconds * 8;
                    scale.Y = scale.X;
                    PrimaryCamera.GetComponent<Transform>().Scale = scale;

                    _atTime += gameTime.ElapsedGameTime.TotalSeconds;
                    
                    if (_atTime >= 4)
                    {
                        _atTime = 0;
                        _state++;
                        _glitch1.Play();
                    }
                    
                    break;
                case 5:
                    // TODO: glitch effect
                    if (_glitch1.State != AudioState.Playing)
                    {
                        _state++;
                        PrimaryCameraSettings.SkyColor = Color.Black;
                        _atLogoSprite.Color = Color.Transparent;
                        _atText.Color = Color.Transparent;
                        _poweredBy.Color = Color.Red;
                        StartTyping(_tsText, _ts, Color.Red);
                    }
                    break;
                case 6:
                    if (_typer == null)
                    {
                        _atFade = 0;
                        _state++;
                        MusicPlayer.PlaySong(_mmTheme, 8);
                    }
                    break;
                case 7:
                    _atFade += (float) gameTime.ElapsedGameTime.TotalSeconds / 8;
                    _atFade = MathHelper.Clamp(_atFade, 0, 1);

                    if (_atFade >= 0.5f)
                    {
                        var realFade = (_atFade - 0.5f) * 2;
                        
                        _tsText.Color = Color.Red * (1 - realFade);
                        _poweredBy.Color = _tsText.Color;

                        PrimaryCameraSettings.SkyColor = Color.Lerp(Color.Black, Color.White, realFade);
                    }

                    if (_atFade >= 1)
                    {
                        MainMenu.ArmFirstDisplay();
                        GoToScene<MainMenu>();
                    }
                    
                    break;
            }

            if (_typer != null)
            {
                _typeTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (_typeTime >= _typeLength)
                {
                    _typer.Text = _textToType;
                    _typer = null;
                    _typeTime = 0;
                    _typeLength = 0;
                }
                else
                {
                    var percentage = (float) (_typeTime / _typeLength);
                    var i = (int) (_textToType.Length * percentage);
                    _typer.Text = _textToType.Substring(0, i);
                }
            }
            
            base.OnUpdate(gameTime);
        }

        private void StartTyping(TextComponent component, string text, Color color)
        {
            _textToType = text;
            _typer = component;
            _typeTime = 0;
            _typeLength = _typeSound.Length.TotalSeconds;

            _typer.Text = string.Empty;
            _typer.Color = color;
            _typeSound.Play();
        }
    }
}