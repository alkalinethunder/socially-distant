using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RedTeam.Net;
using Thundershock;

namespace RedTeam.Game
{
    public class RiskSystem : GlobalComponent
    {
        private List<TraceTimer> _traceTimers = new List<TraceTimer>();

        public bool IsBeingTraced => _traceTimers.Any();

        public double TraceTimeLeft => IsBeingTraced ? _traceTimers.OrderBy(x => x.TimeLeft).First().TimeLeft : 0;

        public event Action TraceRanOut;
        public event Action TraceStarted;
        public void SetTraceTimer(HackStartInfo hack, double traceTime)
        {
            var trace = new TraceTimer
            {
                TimeLeft = traceTime,
                Hack = hack
            };

            _traceTimers.Add(trace);

            TraceStarted?.Invoke();
        }

        protected override void OnUpdate(GameTime gameTime)
        {
            for (var i = 0; i < _traceTimers.Count;i++)
            {
                var timer = _traceTimers[i];
                
                // SUCCESSFUL HACKS
                if (timer.Hack.IsFinished)
                {
                    _traceTimers.RemoveAt(i);
                    i--;
                    continue;
                }
                
                timer.TimeLeft -= gameTime.ElapsedGameTime.TotalSeconds;
                if (timer.TimeLeft <= 0)
                {
                    _traceTimers.RemoveAt(i);
                    i--;
                    TraceRanOut?.Invoke();
                }
            }
        }

        private class TraceTimer
        {
            public double TimeLeft;
            public HackStartInfo Hack;
        }
    }
}