using System;

namespace SociallyDistant.Components
{
    /// <summary>
    /// Contains data necessary for the v-OS Shell to run on an entity.
    /// </summary>
    public class ShellStateComponent
    {
        public string WorkingDirectory { get; set; }
        public string DeviceId { get; set; }
        public int UserId { get; set; }
        public bool IsExecuting { get; set; } = true;
        
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        
        public string TerminalName { get; set; }
        public string DesktopName { get; set; }
        public string WindowManagerName { get; set; }
        public string ShellName { get; set; }
        public bool IsGraphical { get; set; }
        public TimeSpan FrameTime { get; set; }
        public TimeSpan UpTime { get; set; }
    }
}