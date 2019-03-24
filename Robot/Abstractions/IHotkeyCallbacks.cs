using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Robot.Abstractions
{
    public interface IHotkeyCallbacks
    {
        Dictionary<Keys, Action> Hotkeys { get; }

        bool ProcessCallback(Message msg);
        bool Register(Keys key, Action callback);
        void RegisterForm(Form form);
        bool Unregiser(Keys key);
    }
}
