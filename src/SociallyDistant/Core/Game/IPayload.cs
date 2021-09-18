using Thundershock.Gui.Elements.Console;

namespace SociallyDistant.Core.Game
{
    public interface IPayload
    {
        void Init(IConsole console, IProgramContext ctx, IProgramContext player);
        void Update(float deltaTime);
    }
}