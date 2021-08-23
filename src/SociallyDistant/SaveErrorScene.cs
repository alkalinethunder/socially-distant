using System;
using Thundershock;

namespace SociallyDistant
{
    public class SaveErrorScene : Scene
    {
        private static Exception _exceptionToHandle;

        public static void SetException(Exception ex)
        {
            _exceptionToHandle = ex;
        }
    }
}