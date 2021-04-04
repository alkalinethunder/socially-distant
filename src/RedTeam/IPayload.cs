namespace RedTeam
{
    public interface IPayload
    {
        void Init(IConsole console, IRedTeamContext ctx, IRedTeamContext player);
        void Update(float deltaTime);
    }
}