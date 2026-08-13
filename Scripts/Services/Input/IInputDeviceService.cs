using System;

namespace SteelHorse.Framework.Services.Input
{
    public enum InputDeviceMode
    {
        Pointer,
        Navigation
    }

    public interface IInputDeviceService
    {
        InputDeviceMode CurrentMode { get; }

        event Action<InputDeviceMode> ModeChanged;

        void Setup();
    }
}
