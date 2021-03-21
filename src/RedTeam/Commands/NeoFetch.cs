using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace RedTeam.Commands
{
    public class NeoFetch : Command
    {
        public override string Name => "neofetch";
        protected override void Main(string[] args)
        {
            var template = GetTemplate();
            var xnaAsm = typeof(Microsoft.Xna.Framework.Game).Assembly;
            var user = $"{Context.UserName}@{Context.HostName}";
            var line = Repeat('-', user.Length);
            var host = RedTeamPlatform.OSName;
            var version = this.GetType().Assembly.GetName().Version.ToString();
            var kernelname = xnaAsm.GetName().Name;
            var kernelversion = xnaAsm.GetName().Version.ToString();
            var gpu = GraphicsAdapter.DefaultAdapter.Description;
            var memUsed = (GC.GetTotalMemory(false) / 1024 / 1024).ToString();
            var memTotal = RedTeamPlatform.GetTotalSystemMemory().ToString();
            var cpu = RedTeamPlatform.GetProcessorName();

            Substitute(ref template, nameof(cpu), cpu);
            Substitute(ref template, nameof(user), user);
            Substitute(ref template, nameof(line), line);
            Substitute(ref template, nameof(version), version);
            Substitute(ref template, nameof(host), host);
            Substitute(ref template, nameof(kernelname), kernelname);
            Substitute(ref template, nameof(kernelversion), kernelversion);
            Substitute(ref template, nameof(gpu), gpu);
            Substitute(ref template, nameof(memUsed), memUsed);
            Substitute(ref template, nameof(memTotal), memTotal);
            
            Console.WriteLine(template);
        }

        private string Repeat(char ch, int len)
        {
            var str = "";
            for (var i = 0; i < len; i++)
                str += ch;
            return str;
        }
        
        private void Substitute(ref string str, string sub, string value)
        {
            str = str.Replace($"{{{sub}}}", value);
        }
        
        private string GetTemplate()
        {
            var asm = this.GetType().Assembly;
            var resource = asm.GetManifestResourceStream("RedTeam.Resources.NeoFetchTemplate.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8, true);
            return reader.ReadToEnd();
        }
    }
}