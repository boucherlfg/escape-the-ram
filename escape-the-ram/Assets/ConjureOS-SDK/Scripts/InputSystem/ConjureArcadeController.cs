#if ENABLE_INPUT_SYSTEM
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using ConjureOS.Input.LowLevel;
using UnityEngine.InputSystem.Users;

using System.Collections.Generic;
using ArrayHelpers = ConjureOS.Input.Utilities.ArrayHelpers;

namespace ConjureOS.Input.Utilities
{
    /// <summary>
    /// A collection of utility functions for working with arrays.
    /// </summary>
    /// <remarks>
    /// The goal of this collection is to make it easy to use arrays directly rather than resorting to
    /// <see cref="List{T}"/>.
    /// </remarks>
    internal static class ArrayHelpers
    {
        public static int IndexOfReference<TFirst, TSecond>(this TFirst[] array, TSecond value, int count = -1)
            where TSecond : class
            where TFirst : TSecond
        {
            return IndexOfReference(array, value, 0, count);
        }

        public static int IndexOfReference<TFirst, TSecond>(this TFirst[] array, TSecond value, int startIndex, int count)
            where TSecond : class
            where TFirst : TSecond
        {
            if (array == null)
                return -1;

            if (count < 0)
                count = array.Length - startIndex;
            for (var i = startIndex; i < startIndex + count; ++i)
                if (ReferenceEquals(array[i], value))
                    return i;

            return -1;
        }

        public static int AppendWithCapacity<TValue>(ref TValue[] array, ref int count, TValue value, int capacityIncrement = 10)
        {
            if (array == null)
            {
                array = new TValue[capacityIncrement];
                array[0] = value;
                ++count;
                return 0;
            }

            var capacity = array.Length;
            if (capacity == count)
            {
                capacity += capacityIncrement;
                Array.Resize(ref array, capacity);
            }

            var index = count;
            array[index] = value;
            ++count;

            return index;
        }

        public static void EraseAtWithCapacity<TValue>(this TValue[] array, ref int count, int index)
        {
            Debug.Assert(array != null);
            Debug.Assert(count <= array.Length);
            Debug.Assert(index >= 0 && index < count);

            // If we're erasing from the beginning or somewhere in the middle, move
            // the array contents down from after the index.
            if (index < count - 1)
            {
                Array.Copy(array, index + 1, array, index, count - index - 1);
            }

            array[count - 1] = default; // Tail has been moved down by one.
            --count;
        }
    }
}


namespace ConjureOS.Input.LowLevel
{
    /// <summary>
    /// Default state layout for Conjure Arcade controllers.
    /// </summary>
    /// <seealso cref="ConjureArcadeController"/>
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public struct ConjureArcadeControllerState : IInputStateTypeInfo
    {
        public static FourCC Format => new FourCC('C', 'N', 'J', 'A');

        /// <summary>
        /// Button bit mask.
        /// </summary>
        /// <value>Button bit mask.</value>
        /// <seealso cref="ConjureArcadeControllerButton"/>
        /// <seealso cref="Gamepad.buttonSouth"/>
        /// <seealso cref="Gamepad.buttonNorth"/>
        /// <seealso cref="Gamepad.buttonWest"/>
        /// <seealso cref="Gamepad.buttonSouth"/>
        /// <seealso cref="Gamepad.leftShoulder"/>
        /// <seealso cref="Gamepad.rightShoulder"/>
        /// <seealso cref="Gamepad.startButton"/>
        /// <seealso cref="Gamepad.selectButton"/>
        [InputControl(name = "buttonA", layout = "Button", bit = (uint)ConjureArcadeControllerButton.ButtonA, displayName = "Button A", shortDisplayName = "A")]
        [InputControl(name = "buttonB", layout = "Button", bit = (uint)ConjureArcadeControllerButton.ButtonB, displayName = "Button B", shortDisplayName = "B")]
        [InputControl(name = "buttonC", layout = "Button", bit = (uint)ConjureArcadeControllerButton.ButtonC, displayName = "Button C", shortDisplayName = "C")]
        
        [InputControl(name = "button1", layout = "Button", bit = (uint)ConjureArcadeControllerButton.Button1, displayName = "Button 1", shortDisplayName = "1")]
        [InputControl(name = "button2", layout = "Button", bit = (uint)ConjureArcadeControllerButton.Button2, displayName = "Button 2", shortDisplayName = "2")]
        [InputControl(name = "button3", layout = "Button", bit = (uint)ConjureArcadeControllerButton.Button3, displayName = "Button 3", shortDisplayName = "3")]
        
        [InputControl(name = "buttonStart", layout = "Button", bit = (uint)ConjureArcadeControllerButton.ButtonStart, usages = new[] { "Join", "Pause" }, displayName = "Button Start", shortDisplayName = "Start")]
        [InputControl(name = "buttonPower", layout = "Button", bit = (uint)ConjureArcadeControllerButton.ButtonPower, usages = new[] { "Power", "Exit" }, displayName = "Button Power", shortDisplayName = "Power")]
        [FieldOffset(0)]
        public uint buttons;

        /// <summary>
        /// Stick position. Each axis goes from -1 to 1 with
        /// 0 being center position.
        /// </summary>
        /// <value>Left stick position.</value>
        /// <seealso cref="ConjureArcadeController.stick"/>
        [InputControl(layout = "Stick", usage = "Primary2DMotion", processors = "stickDeadzone", displayName = "Stick", shortDisplayName = "S")]
        [FieldOffset(4)]
        public Vector2 stick;

        /// <summary>
        /// State format tag for GamepadState.
        /// </summary>
        /// <value>Returns "CNJA".</value>
        public FourCC format => Format;

        /// <summary>
        /// Create a gamepad state with the given buttons being pressed.
        /// </summary>
        /// <param name="buttons">Buttons to put into pressed state.</param>
        /// <exception cref="ArgumentNullException"><paramref name="buttons"/> is <c>null</c>.</exception>
        public ConjureArcadeControllerState(params GamepadButton[] buttons)
            : this()
        {
            if (buttons == null)
                throw new ArgumentNullException(nameof(buttons));

            foreach (var button in buttons)
            {
                Debug.Assert((int)button < 32, $"Expected button < 32, so we fit into the 32 bit wide bitmask");
                var bit = 1U << (int)button;
                this.buttons |= bit;
            }
        }

        /// <summary>
        /// Set the specific buttons to be pressed or unpressed.
        /// </summary>
        /// <param name="button">A gamepad button.</param>
        /// <param name="value">Whether to set <paramref name="button"/> to be pressed or not pressed in
        /// <see cref="buttons"/>.</param>
        /// <returns>GamepadState with a modified <see cref="buttons"/> mask.</returns>
        public ConjureArcadeControllerState WithButton(GamepadButton button, bool value = true)
        {
            Debug.Assert((int)button < 32, $"Expected button < 32, so we fit into the 32 bit wide bitmask");
            var bit = 1U << (int)button;
            if (value)
                buttons |= bit;
            else
                buttons &= ~bit;
            return this;
        }
    }

    public enum ConjureArcadeControllerButton
    {
        ButtonA = 0,
        ButtonB = 1,
        ButtonC = 2,

        Button1 = 3,
        Button2 = 4,
        Button3 = 5,

        ButtonStart = 6,
        ButtonPower = 7,
    }
}

namespace ConjureOS.Input
{
    [InputControlLayout(stateType = typeof(ConjureArcadeControllerState), displayName = "Conjure Arcade Controller")]
    public class ConjureArcadeController : InputDevice
    {
        public ButtonControl buttonA { get; protected set; }
        public ButtonControl buttonB { get; protected set; }
        public ButtonControl buttonC { get; protected set; }
        
        public ButtonControl button1 { get; protected set; }
        public ButtonControl button2 { get; protected set; }
        public ButtonControl button3 { get; protected set; }
        
        public ButtonControl buttonStart { get; protected set; }
        public ButtonControl buttonPower { get; protected set; }

        public StickControl stick { get; protected set; }

        /// <summary>
        /// Retrieve a gamepad button by its <see cref="ConjureArcadeControllerButton"/> enumeration
        /// constant.
        /// </summary>
        /// <param name="button">Button to retrieve.</param>
        /// <exception cref="ArgumentException"><paramref name="button"/> is not a valid gamepad
        /// button value.</exception>
        public ButtonControl this[ConjureArcadeControllerButton button]
        {
            get
            {
                switch (button)
                {
                    case ConjureArcadeControllerButton.ButtonA: return buttonA;
                    case ConjureArcadeControllerButton.ButtonB: return buttonB;
                    case ConjureArcadeControllerButton.ButtonC: return buttonC;
                    case ConjureArcadeControllerButton.Button1: return button1;
                    case ConjureArcadeControllerButton.Button2: return button2;
                    case ConjureArcadeControllerButton.Button3: return button3;
                    case ConjureArcadeControllerButton.ButtonStart: return buttonStart;
                    case ConjureArcadeControllerButton.ButtonPower: return buttonPower;
                    default:
                        throw new InvalidEnumArgumentException(nameof(button), (int)button, typeof(GamepadButton));
                }
            }
        }

        /// <summary>
        /// The gamepad last used/connected by the player or <c>null</c> if there is no gamepad connected
        /// to the system.
        /// </summary>
        /// <remarks>
        /// When added, a device is automatically made current (see <see cref="InputDevice.MakeCurrent"/>), so
        /// when connecting a gamepad, it will also become current. After that, it will only become current again
        /// when input change on non-noisy controls (see <see cref="InputControl.noisy"/>) is received.
        ///
        /// For local multiplayer scenarios (or whenever there are multiple gamepads that need to be usable
        /// in a concurrent fashion), it is not recommended to rely on this property. Instead, it is recommended
        /// to use <see cref="PlayerInput"/> or <see cref="InputUser"/>.
        /// </remarks>
        /// <seealso cref="InputDevice.MakeCurrent"/>
        /// <seealso cref="all"/>
        public static ConjureArcadeController current { get; private set; }

        /// <summary>
        /// A list of gamepads currently connected to the system.
        /// </summary>
        /// <value>All currently connected gamepads.</value>
        /// <remarks>
        /// Does not cause GC allocation.
        ///
        /// Do <em>not</em> hold on to the value returned by this getter but rather query it whenever
        /// you need it. Whenever the gamepad setup changes, the value returned by this getter
        /// is invalidated.
        /// </remarks>
        /// <seealso cref="current"/>
        public new static ReadOnlyArray<ConjureArcadeController> all => new(s_Gamepads, 0, s_GamepadCount);

        /// <inheritdoc />
        protected override void FinishSetup()
        {
            buttonA = GetChildControl<ButtonControl>("buttonA");
            buttonB = GetChildControl<ButtonControl>("buttonB");
            buttonC = GetChildControl<ButtonControl>("buttonC");
            
            button1 = GetChildControl<ButtonControl>("button1");
            button2 = GetChildControl<ButtonControl>("button2");
            button3 = GetChildControl<ButtonControl>("button3");

            buttonStart = GetChildControl<ButtonControl>("buttonStart");
            buttonPower = GetChildControl<ButtonControl>("buttonPower");

            stick = GetChildControl<StickControl>("stick");

            base.FinishSetup();
        }

        /// <summary>
        /// Make the gamepad the <see cref="current"/> gamepad.
        /// </summary>
        /// <remarks>
        /// This is called automatically by the system when there is input on a gamepad.
        /// </remarks>
        public override void MakeCurrent()
        {
            base.MakeCurrent();
            current = this;
        }

        /// <summary>
        /// Called when the gamepad is added to the system.
        /// </summary>
        protected override void OnAdded()
        {
            ArrayHelpers.AppendWithCapacity(ref s_Gamepads, ref s_GamepadCount, this);
        }

        /// <summary>
        /// Called when the gamepad is removed from the system.
        /// </summary>
        protected override void OnRemoved()
        {
            if (current == this)
                current = null;

            // Remove from `all`.
            var index = ArrayHelpers.IndexOfReference(s_Gamepads, this, s_GamepadCount);
            if (index != -1)
                ArrayHelpers.EraseAtWithCapacity(s_Gamepads, ref s_GamepadCount, index);
            else
            {
                Debug.Assert(false,
                    $"Gamepad {this} seems to not have been added but is being removed (gamepad list: {string.Join(", ", all)})"); // Put in else to not allocate on normal path.
            }
        }

        private static int s_GamepadCount;
        private static ConjureArcadeController[] s_Gamepads;
    }
}
#endif