using System;
using Microsoft.Xna.Framework;

namespace RedTeam
{
    public abstract class GlobalComponent
    {
        protected RedTeamGame Game { get; private set; }
        
        public void Initialize(RedTeamGame game)
        {
            Game = game ?? throw new ArgumentNullException(nameof(game));

            OnLoad();
        }

        public void Unload()
        {
            OnUnload();
            Game = null;
        }

        public void Update(GameTime gameTime)
        {
            OnUpdate(gameTime);
        }
        
        protected virtual void OnLoad() {}
        protected virtual void OnUnload() {}
        
        protected virtual void OnUpdate(GameTime gameTime) {}
        
    }
}