#if VRC_SDK_VRCSDK3
using System;

namespace BlackStartX.GestureManager.Editor.Modules.Vrc3
{
    public class Vrc3Warning
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string Button;
        public readonly Action Action;
        public readonly bool Closable;

        public Vrc3Warning(string title, string description, bool closable = true, string button = null, Action action = null)
        {
            Title = title;
            Description = description;
            Closable = closable;
            Button = button;
            Action = action;
        }

        public static readonly Vrc3Warning PausedEditor = new("Editor Paused", "The editor is in pause, animator will be so as well.", false);
    }
}
#endif