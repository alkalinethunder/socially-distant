﻿using System;
using System.IO;
using System.Text;

namespace RedTeam.Commands
{
    public class Fortune : Command
    {
        public override string Name => "fortune";

        private string[] GetFortunes()
        {
            var skip = "###";
            var sep = "%";

            var res = this.GetType().Assembly.GetManifestResourceStream("RedTeam.Resources.Fortunes.txt");
            using var reader = new StreamReader(res, Encoding.UTF8, true);

            var raw = reader.ReadToEnd();

            var skipIndex = raw.IndexOf(skip);
            var fortunesRaw = raw.Substring(skipIndex + skip.Length).Trim();

            var fortunes =
                fortunesRaw.Split(sep, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            return fortunes;
        }
        
        protected override void Main(string[] args)
        {
            var fortunes = GetFortunes();
            var random = new Random();

            var fortune = fortunes[random.Next(fortunes.Length)];
            
            Console.Write(fortune);
        }
    }
}