using System;
using Thundershock;

namespace RedTeam
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            EntryPoint.RegisterApp<ContentEditorApp>("editor");
            EntryPoint.Run<RedTeamApp>(args);
        }
    }
}
