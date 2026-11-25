using System;
using ClientPlugin.Policy;
using ClientPlugin.Settings.Tools;
using Keen.VRage.Core.Game.Components;
using Keen.VRage.Core.Game.Systems;
using Keen.VRage.Core.Input;
using Keen.VRage.Input;

namespace ClientPlugin.Game;

/// <summary>
/// Keyboard and mouse wheel, read once per frame.
/// Game API: Session.TryGet&lt;IInputManager&gt;() gives .Keyboard and .Mouse (IInputDevice), same as the
/// Head Tracking SE2 plugin. DigitalInput(vk, GenericDeviceClass.Keyboard).IsActive(device) for keys,
/// IInputDevice.GetAnalogState(MouseInputs.VerticalWheel) for the wheel (notches since the last input update).
/// </summary>
internal sealed class InputReader
{
    private bool toggleLast, nextLast, previousLast, escapeLast, rightButtonLast;
    private bool screenWasOpen;
    private float wheelAccumulator;

    public bool Toggle { get; private set; }
    public bool Next { get; private set; }
    public bool Previous { get; private set; }

    /// <summary>
    /// Esc pressed this frame with no screen open now or in the last frame (there Esc closes that screen).
    /// Read directly because in turret control Esc did not reach the game's menu hotkey in testing.
    /// </summary>
    public bool Escape { get; private set; }

    /// <summary>Right mouse button pressed this frame (turret reload).</summary>
    public bool RightClick { get; private set; }

    /// <summary>Whole wheel notches this frame. Positive = zoom in.</summary>
    public int WheelNotches { get; private set; }

    public void Read(Session session)
    {
        Toggle = Next = Previous = Escape = RightClick = false;
        WheelNotches = 0;

        var inputManager = session.TryGet<IInputManager>();
        var keyboard = inputManager?.Keyboard;
        if (keyboard != null)
        {
            var config = Config.Current;
            Toggle = Pressed(keyboard, config.ToggleKey, ref toggleLast);
            Next = Pressed(keyboard, config.NextKey, ref nextLast);
            Previous = Pressed(keyboard, config.PreviousKey, ref previousLast);
            var escapeDown = KeyboardInputs.Escape.IsActive(keyboard);
            Escape = escapeDown && !escapeLast;
            escapeLast = escapeDown;
        }

        var mouse = inputManager?.Mouse;
        if (mouse != null)
        {
            var rightDown = MouseInputs.Right.IsActive(mouse);
            RightClick = rightDown && !rightButtonLast;
            rightButtonLast = rightDown;

            // The game's MouseDevice reports the wheel as notches moved since the last input update
            // (raw value / 120), 0 when the wheel did not move.
            var raw = mouse.GetAnalogState(MouseInputs.VerticalWheel);
            if (raw != 0f)
                WheelNotches = Zoom.TakeNotches(ref wheelAccumulator, raw, 1f);
        }

        // A screen with a mouse cursor is open (terminal, inventory, menu, settings): keys and wheel belong to it.
        // The key state above is still tracked, so a key held while the screen closes does not fire.
        var screenOpen = UiState.IsCursorScreenOpen();
        if (screenWasOpen) Escape = false;
        screenWasOpen = screenOpen;
        if (screenOpen)
        {
            Toggle = Next = Previous = Escape = RightClick = false;
            WheelNotches = 0;
            wheelAccumulator = 0f;
        }
    }

    private static bool Pressed(IInputDevice keyboard, Binding binding, ref bool last)
    {
        var pressed = IsPressed(keyboard, binding);
        var justPressed = pressed && !last;
        last = pressed;
        return justPressed;
    }

    private static bool IsPressed(IInputDevice keyboard, Binding binding) =>
        binding.IsBound &&
        new DigitalInput(binding.Vk, GenericDeviceClass.Keyboard).IsActive(keyboard) &&
        KeyboardInputs.Control.IsActive(keyboard) == binding.Ctrl &&
        KeyboardInputs.Alt.IsActive(keyboard) == binding.Alt &&
        KeyboardInputs.Shift.IsActive(keyboard) == binding.Shift;
}
