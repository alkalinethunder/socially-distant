using Thundershock.Core;
using Thundershock.Core.Rendering;
using Thundershock.Gui;

namespace SociallyDistant.Gui.Windowing
{
    public abstract class WindowTheme
    {
        private bool _disposed = false;

        public void Load(GraphicsProcessor gpu)
        {
            OnLoad(gpu);
        }
        
        public abstract Padding GetClientPadding(WindowFrame win);

        public abstract void PaintWindow(GameTime gameTime, GuiRenderer renderer, WindowFrame win);

        public void Dispose()
        {
            if (!_disposed)
            {
                Dispose(true);
            }
        }

        protected virtual void OnLoad(GraphicsProcessor gpu) {}
        
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}