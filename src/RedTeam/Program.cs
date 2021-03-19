using System;

namespace RedTeam
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new RedTeamGame();
            game.Run();
        }
    }
}
