using System;
using System.Collections.Generic;
using System.Diagnostics;
using Thundershock.Core.Rendering;

namespace SociallyDistant.Core
{
    public class Notification
    {
        public string Title { get; }
        public string Message { get; }
        public Texture2D Icon { get; }
        public double Time { get; }
        public Dictionary<string, Action> Actions { get; } = new();

        public Notification(string title, string message, double time, Texture2D icon = null)
        {
            Title = title;
            Message = message;
            Icon = icon;
            Time = time;
        }

        public void AddButton(string buttonText, Action action = null)
        {
            Debug.Assert(!Actions.ContainsKey(buttonText));

            Actions.Add(buttonText, action);
        }
    }
}