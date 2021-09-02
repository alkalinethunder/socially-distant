using System;
using System.Collections.Generic;
using SociallyDistant.Core.Windowing;
using Thundershock;
using Thundershock.Core;

namespace SociallyDistant.Core.Displays
{
    public abstract class DisplayWindow
    {
        private WindowFrame _window;
        private IRedTeamContext _ctx;
        
        protected DisplayWindow() {}

        protected IRedTeamContext Context => _ctx;
        
        public WindowFrame Window => _window;

        protected abstract void Main();

        public static DisplayWindow Create(WindowFrame window, IRedTeamContext ctx, Type self)
        {
            var display = (DisplayWindow) Activator.CreateInstance(self, true);
            
            display._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            display._window = window ?? throw new ArgumentNullException(nameof(window));

            display.Main();
            
            return display;
        }
        
        public static T Create<T>(WindowFrame window, IRedTeamContext ctx) where T : DisplayWindow, new()
        {
            var display = new T();

            display._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            display._window = window ?? throw new ArgumentNullException(nameof(window));

            display.Main();
            
            return display;
        }

        public virtual void Update(GameTime gameTime)
        {
            
        }
        
    }
}