using SociallyDistant.Core.Windowing;
using Thundershock.Core;

namespace SociallyDistant.Core.Gui.Elements
{
    public interface IPaneLayout
    {
        Color Color { get; set; }
        WindowManager WindowManager { get; }
    }
}