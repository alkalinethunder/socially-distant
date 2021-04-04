﻿using Thundershock.IO;

namespace RedTeam.IO
{
    public static class FileSystemExtensions
    {
        public static IConsole CreateFileConsole(this FileSystem fs, IConsole input, string path, bool append)
        {
            var s = fs.OpenFile(path, append);
            if (!append)
                s.SetLength(0);
            return new FileConsole(input, s);
        }

    }
}