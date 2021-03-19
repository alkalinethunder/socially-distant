using Microsoft.Xna.Framework.Input;

namespace RedTeam.Input
{
    public class MouseButtonEventArgs : MouseEventArgs
    {
        public ButtonState State { get; }
        public MouseButton Button { get; }

        public MouseButtonEventArgs(MouseState mouseState, MouseButton button, ButtonState state) : base(mouseState)
        {
            Button = button;
            State = state;
        }
    }
}