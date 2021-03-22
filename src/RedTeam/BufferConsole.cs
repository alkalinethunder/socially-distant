using System;
using System.Collections.Generic;

namespace RedTeam
{
    public class BufferConsole : IConsole
    {
        private string _buffer = string.Empty;
        
        public IAutoCompleteSource AutoCompleteSource { get; set; }
        
        public void Write(object value)
        {
            Write(value?.ToString() ?? "null");
        }

        public void Write(string valuue)
        {
            _buffer += valuue;
        }

        public void WriteLine(object value)
        {
            Write(value);
            WriteLine();
        }

        public void WriteLine(string value)
        {
            Write(value);
            WriteLine();
        }

        public void Write(string format, params object[] values)
        {
            Write(string.Format(format, values));
        }


        public void WriteLine(string format, params object[] values)
        {
            Write(format, values);
            WriteLine();
        }

        public void WriteLine()
        {
            Write(Environment.NewLine);
        }

        public void Clear()
        {
            // Stub. Buffers can't clear.
        }

        public bool GetLine(out string text)
        {
            // short-hand.
            var nl = Environment.NewLine;

            if (_buffer.Contains(nl))
            {
                var index = _buffer.IndexOf(nl);

                var line = _buffer.Substring(0, index);

                _buffer = _buffer.Substring(line.Length + nl.Length);

                text = line;
                return true;
            }

            text = string.Empty;
            return false;
        }

        public bool GetCharacter(out char character)
        {
            if (_buffer.Length > 0)
            {
                character = _buffer[0];
                _buffer = _buffer.Substring(1);
                return true;
            }

            character = '\0';
            return false;
        }
    }
}