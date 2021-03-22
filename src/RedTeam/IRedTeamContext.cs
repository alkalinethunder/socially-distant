namespace RedTeam
{
    public interface IRedTeamContext
    {
        string UserName { get; }
        string HostName { get; }
        string HomeDirectory { get; }
        string Terminal { get; }
        string Shell { get; }
        string WindowManager { get; }
        string DesktopEnvironment { get; }
    }
}