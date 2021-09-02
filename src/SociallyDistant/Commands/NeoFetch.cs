using System;
using System.Globalization;
using System.IO;
using System.Text;
using Thundershock;
using Thundershock.Core;

namespace SociallyDistant.Commands
{
    public class NeoFetch : Command
    {
        public override string Name => "neofetch";
        public override string Description => "Print fancy system information.";
        
        protected override void Main(string[] args)
        {
            var gameUptime = Context.Uptime;

            var uptime = $"{(int) gameUptime.TotalHours} hours, {gameUptime.Minutes} minutes, {gameUptime.Seconds} seconds";
            var template = GetTemplate();
            var xnaAsm = typeof(GraphicalAppBase).Assembly;
            var user = $"{Context.UserName}@{Context.HostName}";
            var line = Repeat('-', user.Length);
            var host = ThundershockPlatform.OsName;
            var version = GetType().Assembly.GetName().Version.ToString();
            var kernelname = xnaAsm.GetName().Name;
            var kernelversion = xnaAsm.GetName().Version.ToString();
            var gpu = GamePlatform.GraphicsCardDescription;
            var memUsed = (GC.GetTotalMemory(false) / 1024 / 1024).ToString();
            var memTotal = ThundershockPlatform.GetTotalSystemMemory().ToString();
            var cpu = ThundershockPlatform.GetProcessorName();
            var term = Context.Terminal;
            var shell = Context.Shell;
            var wm = Context.WindowManager;
            var de = Context.DesktopEnvironment;
            var pkgs = "0 (upgrades) | 0 (rpkg) | 0 (modldr)";
            var fps = Math.Round(1 / Context.FrameTime.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            var width = GamePlatform.GraphicsProcessor.ViewportBounds.Width.ToString();
            var height = GamePlatform.GraphicsProcessor.ViewportBounds.Height.ToString();
            
            Substitute(ref template, nameof(width), width);
            Substitute(ref template, nameof(height), height);
            Substitute(ref template, nameof(fps), fps);
            Substitute(ref template, nameof(pkgs), pkgs);
            Substitute(ref template, nameof(uptime), uptime);
            Substitute(ref template, nameof(term), term);
            Substitute(ref template, nameof(shell), shell);
            Substitute(ref template, nameof(wm), wm);
            Substitute(ref template, nameof(de), de);
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
            var sb = new StringBuilder();
            var asm = GetType().Assembly;
            var resource = asm.GetManifestResourceStream("SociallyDistant.Resources.NeoFetchTemplate.txt");
            using var reader = new StreamReader(resource, Encoding.UTF8, true);
            var raw = reader.ReadToEnd();

            var lines = raw.Split(Environment.NewLine);

            foreach (var line in lines)
            {
                if (line.Contains("///"))
                {
                    var stripped = line.Substring(0, line.IndexOf("///"));
                    sb.AppendLine(stripped);
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }
    }
}