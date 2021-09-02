using System;
using System.IO;
using Thundershock.Gui.Elements.Console;

namespace SociallyDistant.Core.IO
{
    public class FileConsole : IConsole, IDisposable
    {
        private IConsole _input;
        private Stream _file;
        private StreamWriter _writer;

        public void Dispose()
        {
            _writer.Close();
            _file.Close();

            _writer = null;
            _file = null;
            _input = null;
        }
        
        public FileConsole(IConsole input, Stream stream)
        {
            _input = input;
            _file = stream;

            _writer = new StreamWriter(_file);
            _writer.AutoFlush = true;
        }

        public IAutoCompleteSource AutoCompleteSource
        {
            get => _input.AutoCompleteSource;
            set => _input.AutoCompleteSource = value;
        }

        public void Write(object value)
        {
            var text = "null";
            if (value != null)
                text = value.ToString();
            Write(text);
        }

        public void Write(string value)
        {
            _writer.Write(value);
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
            WriteLine(string.Format(format, values));
        }

        public void WriteLine()
        {
            _writer.WriteLine();
        }

        public void Clear()
        {
            // stub.
        }

        public bool GetLine(out string text)
        {
            return _input.GetLine(out text);
        }

        public bool GetCharacter(out char character)
        {
            return _input.GetCharacter(out character);
        }
    }
}