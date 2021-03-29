using System;
using Thundershock;

namespace RedTeam
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            EntryPoint.RegisterApp("editor", typeof(ContentEditorApp));
            EntryPoint.Run<RedTeamApp>(args);
        }
    }
}
