using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using RedTeam.Game;
using RedTeam.Net;
using Thundershock;
using Thundershock.Components;
using Thundershock.Debugging;
using Thundershock.Input;

namespace RedTeam.Components
{
    public class TraceTimerComponent : SceneComponent
    {
        private bool _flashing;
        private int _flashes;
        private double _flashTick;
        private SoundEffect _fuck;
        private double _nextFuck;
        private SoundEffect _tickTock;
        private double _pulse;
        private double _nextTick;
        private RiskSystem _risk;
        private TextComponent _panicTimer;
        private VorbisPlayer _glitchie;
        private bool _glitching = false;
        private int _glitchLevel = 0;
        private VorbisPlayer _vorbisPlayer;
        
        protected override void OnLoad()
        {
            _fuck = App.Content.Load<SoundEffect>("Sounds/Panic/OhFuck");
            _tickTock = App.Content.Load<SoundEffect>("Sounds/Panic/PanicTimerWarning");
            _vorbisPlayer = Scene.AddComponent<VorbisPlayer>();
            _risk = App.GetComponent<RiskSystem>();
            _panicTimer = Scene.AddComponent<TextComponent>();
            _glitchie = Scene.AddComponent<VorbisPlayer>();

            _panicTimer.Font = App.Content.Load<SpriteFont>("Fonts/PanicTimer");

            _panicTimer.Origin = Vector2.One;
            _panicTimer.Pivpt = Vector2.One;
            _panicTimer.Position = Vector2.Zero - new Vector2(20, 20);
            
            _risk.TraceRanOut += Panic;

            _risk.TraceStarted += SetWarningTimer;
            _risk.TraceStarted += SetFuckTimer;
            
            App.GetComponent<CheatManager>().AddCheat("ritchie", Ritchie);
            
            base.OnLoad();
        }

        private void Ritchie()
        {
            // clear all active traces.
            _risk.ClearAllTraces();
            
            // Play the ritchie sound
            _vorbisPlayer.OpenResource(this.GetType().Assembly, "RedTeam.Resources.Document1.ogg");
            
            // create a fake trace, lol.
            _risk.SetTraceTimer(new HackStartInfo(null, null, null), _vorbisPlayer.Duration);
            
            // hahaha
            App.GetComponent<InputManager>().EnableInput = false;
        }

        private void SetWarningTimer()
        {
            var timeLeft = _risk.TraceTimeLeft;
            if (timeLeft >= 0.5)
            {
                var highInterval = Math.Round(timeLeft - 0.5) + 0.5;
                _nextTick = highInterval - 0.5;
            }
            else
            {
                _nextTick = timeLeft / 2;
            }
        }

        private void StartFlash()
        {
            _flashes = 10;
            _flashing = true;
            _flashTick = 0.05;
        }
        
        private void SetFuckTimer()
        {
            var timeLeft = _risk.TraceTimeLeft;
            if (timeLeft > 30)
            {
                var highInterval = Math.Round(timeLeft / 30) * 30;
                _nextFuck = highInterval - 30;
            }
            else
            {
                _nextFuck = timeLeft / 2;
            }

        }

        private void Panic()
        {
            _glitching = true;
            _glitchLevel = 0;
            _glitchie.OpenResource(this.GetType().Assembly, "RedTeam.Resources.Audio.ThundershockGlitch.ogg");
            
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            _panicTimer.Visible = _risk.IsBeingTraced;
            
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

            if (_risk.IsBeingTraced)
            {
                _pulse -= gameTime.ElapsedGameTime.TotalSeconds * _tickTock.Duration.TotalSeconds;
                _panicTimer.Color = Color.Lerp(Color.White, Color.Red, MathHelper.Clamp((float) _pulse, 0, 1));
                
                if (ts.TotalSeconds <= _nextTick)
                {
                    _tickTock.Play();
                    _pulse = 1;
                    SetWarningTimer();
                }

                if (ts.TotalSeconds < _nextFuck)
                {
                    _fuck.Play();
                    SetFuckTimer();
                    StartFlash();
                }
                
                if (_flashing)
                {
                    _flashTick -= gameTime.ElapsedGameTime.TotalSeconds;
                    if (_flashTick <= 0)
                    {
                        _flashes--;
                        _flashTick = 0.05;
                        if (_flashes < 0)
                            _flashing = false;
                    }

                    _panicTimer.Visible = (_flashes % 2) == 0;
                }
            }

            base.OnUpdate(gameTime);
        }
    }
}