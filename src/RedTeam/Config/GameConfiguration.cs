namespace RedTeam.Config
{
    public class GameConfiguration
    {
        public bool IsFullscreen { get; set; } = true;
        public bool VSync { get; set; } = true;
        public bool FixedTimeStepping { get; set; } = true;
        public bool SwapMouseButtons { get; set; } = true;
        
        public string Resolution { get; set; } = string.Empty;
    }
}