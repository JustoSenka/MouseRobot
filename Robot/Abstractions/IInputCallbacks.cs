using Robot.Input;
using System;
using System.ComponentModel;

namespace Robot.Abstractions
{
    public interface IInputCallbacks
    {
        AsyncOperation AsyncOperationOnUI { set; }

        event Action<KeyEvent> InputEvent;

        void ForgetAll();
        void Init();
    }
}