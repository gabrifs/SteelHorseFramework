using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace SteelHorse.Framework.Services.Input
{
    // Classifies input at the raw device level (Mouse vs. Keyboard/Gamepad) instead of
    // hooking a scene's InputSystemUIInputModule action references the way MenuNavigator
    // reads its cancel action: this service lives on the persistent GameManagers singleton
    // and has to keep tracking correctly across scene loads, while EventSystem and its
    // InputSystemUIInputModule are recreated per scene. Device-level classification also
    // means it works no matter which InputActionAsset a scene's EventSystem is configured
    // with, since it never reads bindings/actions at all.
    public class InputDeviceService : MonoBehaviour, IInputDeviceService
    {
        // Filters out gamepad stick drift/noise so resting the controller doesn't flip
        // the mode; mouse and keyboard controls are digital/delta-based and don't need this.
        private const float GamepadActuationThreshold = 0.2f;

        public InputDeviceMode CurrentMode { get; private set; } = InputDeviceMode.Pointer;

        public event Action<InputDeviceMode> ModeChanged;

        public void Setup() => InputSystem.onEvent += OnInputEvent;

        private void OnDestroy() => InputSystem.onEvent -= OnInputEvent;

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;

            switch (device)
            {
                case Mouse:
                    if (HasActuation(eventPtr, device))
                        SetMode(InputDeviceMode.Pointer);
                    break;

                case Keyboard:
                    if (HasActuation(eventPtr, device))
                        SetMode(InputDeviceMode.Navigation);
                    break;

                case Gamepad:
                    if (HasActuation(eventPtr, device, GamepadActuationThreshold))
                        SetMode(InputDeviceMode.Navigation);
                    break;
            }
        }

        // Only counts a changed control as actuation if it's a button being pressed (not
        // released) or any non-button control (mouse delta, stick position, ...) moving past
        // the threshold - so releasing a key/button doesn't spuriously flip the mode back.
        private static bool HasActuation(InputEventPtr eventPtr, InputDevice device, float magnitudeThreshold = 0f)
        {
            foreach (var control in eventPtr.EnumerateChangedControls(device, magnitudeThreshold))
            {
                if (control is ButtonControl button)
                {
                    // Can't read button.isPressed here - it reflects the control's value before
                    // this event is applied, since onEvent fires ahead of the event being
                    // committed to device state. Read the button's value out of the event itself.
                    if (button.IsValueConsideredPressed(button.ReadValueFromEvent(eventPtr)))
                        return true;
                    continue;
                }

                return true;
            }

            return false;
        }

        private void SetMode(InputDeviceMode mode)
        {
            if (CurrentMode == mode)
                return;

            CurrentMode = mode;
            ModeChanged?.Invoke(mode);
        }
    }
}
