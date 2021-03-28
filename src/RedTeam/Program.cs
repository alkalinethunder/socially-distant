using System;
using Thundershock;

namespace RedTeam
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // TODO: content editor
            EntryPoint.Run<RedTeamApp>(args);
        }
    }
}
