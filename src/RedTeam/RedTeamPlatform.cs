using System;
using System.Collections.Generic;
using BindingFlags = System.Reflection.BindingFlags;

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
        
        public static IEnumerable<Type> GetAllTypes<T>()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (!typeof(T).IsAssignableFrom(type))
                        continue;

                    if (type.GetConstructor(Type.EmptyTypes) != null)
                        yield return type;
                }
            }
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