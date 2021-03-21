using System;

namespace RedTeam
{
    public static class RedTeamPlatform
    {
        public static Platform GetCurrentPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.MacOSX:
                    return Platform.MacOS;
                case PlatformID.WinCE:
                case PlatformID.Win32NT:
                case PlatformID.Win32Windows:
                case PlatformID.Win32S:
                case PlatformID.Xbox:
                    return Platform.Windows;
                case PlatformID.Unix:
                    return Platform.Linux;
            }

            return Platform.Unknown;
        }
        
        public static bool IsPlatform(Platform platform)
        {
            return GetCurrentPlatform() == platform;
        }
    }

    public enum Platform
    {
        Windows,
        MacOS,
        Linux,
        Unknown,
    }
}