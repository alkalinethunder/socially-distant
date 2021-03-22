namespace RedTeam
{
    public class Pipe : IConsole
    {
        private IConsole _input;
        private IConsole _output;

        public Pipe(IConsole input, IConsole output)
        {
            _input = input;
            _output = output;
        }

        public IConsole Input
        {
            get => _input;
            set => _input = value;
        }
        
        public IConsole Output
        {
            get => _output;
            set => _output = value;
        }
        
        
        
        public IAutoCompleteSource AutoCompleteSource { get; set; }
        
        public void Write(object value)
        {
            _output.Write(value);
        }

        public void Write(string valuue)
        {
            _output.Write(valuue);
        }

        public void WriteLine(object value)
        {
            _output.WriteLine(value);
        }

        public void WriteLine(string value)
        {
            _output.WriteLine(value);
        }

        public void Write(string format, params object[] values)
        {
            _output.Write(format, values);
        }

        public void WriteLine(string format, params object[] values)
        {
            _output.WriteLine(format, values);
        }

        public void WriteLine()
        {
            _output.WriteLine();
        }

        public void Clear()
        {
            _output.Clear();
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