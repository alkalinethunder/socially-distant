using Thundershock.Gui.Elements.Console;

namespace SociallyDistant.Core
{
    public interface IPayload
    {
        void Init(IConsole console, IRedTeamContext ctx, IRedTeamContext player);
        void Update(float deltaTime);
    }
}