using SociallyDistant.Gui.Windowing;
using Thundershock.Core;

namespace SociallyDistant.Gui.Elements
{
    public interface IPaneLayout
    {
        Color Color { get; set; }
        WindowManager WindowManager { get; }
    }
}