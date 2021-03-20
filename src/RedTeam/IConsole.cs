﻿namespace RedTeam
{
    public interface IConsole
    {
        void Write(object value);
        void WriteLine(object value);
        void Write(string format, params object[] values);
        void WriteLine(string format, params object[] values);
        void WriteLine();
        void Clear();

        bool GetLine(out string text);
        bool GetCharacter(out char character);
    }
}