using System;
using SociallyDistant.Core.Mail;
using Thundershock.IO;

namespace SociallyDistant.Core
{
    public interface IProgramContext
    {
        Mailbox Mailbox { get; }
        public string WorkingDirectory { get; }
        public int ScreenWidth { get; }
        public int ScreenHeight { get; }
        public TimeSpan FrameTime { get; }
        public TimeSpan Uptime { get; }
        public bool IsGraphical { get; }
        public FileSystem Vfs { get; }
        string UserName { get; }
        string HostName { get; }
        string HomeDirectory { get; }
        string Terminal { get; }
        string Shell { get; }
        string WindowManager { get; }
        string DesktopEnvironment { get; }

        void ShutDown();

        void SetWorkingDirectory(string work);
    }
}