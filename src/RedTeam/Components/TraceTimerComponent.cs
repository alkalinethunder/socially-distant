using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Game;
using Thundershock;
using Thundershock.Components;

namespace RedTeam.Components
{
    public class TraceTimerComponent : SceneComponent
    {
        private RiskSystem _risk;
        private TextComponent _panicTimer;
        private VorbisPlayer _glitchie;
        private bool _glitching = false;
        private int _glitchLevel = 0;
        
        protected override void OnLoad()
        {
            _risk = App.GetComponent<RiskSystem>();
            _panicTimer = Scene.AddComponent<TextComponent>();
            _glitchie = Scene.AddComponent<VorbisPlayer>();

            _panicTimer.Font = App.Content.Load<SpriteFont>("Fonts/PanicTimer");

            _panicTimer.Origin = Vector2.One;
            _panicTimer.Pivpt = Vector2.One;
            _panicTimer.Position = Vector2.Zero - new Vector2(20, 20);
            
            _risk.TraceRanOut += Panic;
            
            base.OnLoad();
        }

        private void Panic()
        {
            _glitching = true;
            _glitchLevel = 0;
            _glitchie.OpenResource(this.GetType().Assembly, "RedTeam.Resources.Audio.ThundershockGlitch.ogg");
            
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            if (_glitching)
            {
                if (!_glitchie.IsPlaying)
                {
                    if (_glitchLevel == 0)
                    {
                        _glitchie.OpenResource(this.GetType().Assembly, "RedTeam.Resources.Audio.RedTeamGlitch.ogg");
                        _glitchLevel++;
                    }
                    else
                    {
                        Scene.Game.Exit();
                    }
                }
                
                Scene.Game.PostProcessSettings.EnableGlitch = true;
                Scene.Game.PostProcessSettings.GlitchIntensity = _glitchie.Power;
                Scene.Game.PostProcessSettings.GlitchSkew = _glitchie.Power / 2;
            }
            else
            {
                Scene.Game.PostProcessSettings.EnableGlitch = false;
            }
            
            var /*josh*/
                ts = TimeSpan.FromSeconds(_risk.TraceTimeLeft);

            if (ts.TotalMinutes < 1)
            {
                _panicTimer.Text = string.Format("{0:N2}", ts.TotalSeconds);
            }
            else
            {
                _panicTimer.Text = string.Format("{0}:{1}", ts.Minutes, ts.Seconds.ToString("00"));
            }

            base.OnUpdate(gameTime);
        }
    }
}