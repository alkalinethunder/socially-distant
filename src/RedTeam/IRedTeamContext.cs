namespace RedTeam
{
    public interface IRedTeamContext
    {
        string UserName { get; }
        string HostName { get; }
        string HomeDirectory { get; }
    }
}