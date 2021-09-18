using System;
using SociallyDistant.Core;
using SociallyDistant.Gui.Windowing;
using Thundershock.Core;

namespace SociallyDistant.Shell.Displays
{
    public abstract class DisplayWindow
    {
        private WindowFrame _window;
        private IProgramContext _ctx;
        
        protected DisplayWindow() {}

        protected IProgramContext Context => _ctx;
        
        public WindowFrame Window => _window;

        protected abstract void Main();

        public static DisplayWindow Create(WindowFrame window, IProgramContext ctx, Type self)
        {
            var display = (DisplayWindow) Activator.CreateInstance(self, true);
            
            display._ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            display._window = window ?? throw new ArgumentNullException(nameof(window));

            display.Main();
            
            return display;
        }
        
        public static T Create<T>(WindowFrame window, IProgramContext ctx) where T : DisplayWindow, new()
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