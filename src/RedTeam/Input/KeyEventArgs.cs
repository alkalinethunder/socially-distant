using System;
using Microsoft.Xna.Framework.Input;

namespace RedTeam.Input
{
    public class KeyEventArgs : EventArgs
    {
        public Keys Key { get; }

        public KeyEventArgs(Keys key)
        {
            Key = key;
        }
    }
}