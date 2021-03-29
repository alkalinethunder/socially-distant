using System;
using System.IO;
using System.Text;
using Thundershock.Debugging;

namespace RedTeam.IO
{
    public class KmsgLogOutput : ILogOutput
    {
        private static byte[] _kmsgBuffer = Array.Empty<byte>();
        private static int _pos = 0;

        public static MemoryStream OpenLogStream()
        {
            return new MemoryStream(_kmsgBuffer);
        }
        
        public void Log(string message, LogLevel logLevel)
        {
            var bytes = Encoding.UTF8.GetBytes(message + Environment.NewLine);
            var end = _pos + bytes.Length;

            if (end > _kmsgBuffer.Length)
            {
                Array.Resize(ref _kmsgBuffer, end);
            }

            for (var i = 0; i < bytes.Length; i++)
            {
                _kmsgBuffer[_pos] = bytes[i];
                _pos++;
            }
        }
    }
}