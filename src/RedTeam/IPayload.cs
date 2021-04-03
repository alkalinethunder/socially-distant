namespace RedTeam
{
    public interface IPayload
    {
        void Init(IConsole console, IRedTeamContext ctx);
        void Update(float deltaTime);
    }
}