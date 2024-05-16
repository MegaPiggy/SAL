using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;
using SALT.Extensions;
using SALT.Utils;

namespace SALT
{
    public class UserInputService : USingleton<UserInputService>
    {
        protected Vector2 mousePos = Vector2.zero;

        internal static string Platform { get; private set; }

        private static bool mouseVisible = false;
        public static bool MouseVisible { get => mouseVisible; set => mouseVisible = value;  }

        public static int GetSystemBuildNumber() => System.Environment.OSVersion.Version.Build;

        internal static string PluginFileExtension() => ".dll";

        private static UserInputType lastInputType = UserInputType.NONE;
        public static UserInputType GetLastInputType() => lastInputType;
        public static bool TouchEnabled => GetLastInputType() == UserInputType.TOUCH;

        public static List<KeyCode> KeyboardCodes = new List<KeyCode>()
        {
            KeyCode.A,
            KeyCode.B,
            KeyCode.C,
            KeyCode.D,
            KeyCode.E,
            KeyCode.F,
            KeyCode.G,
            KeyCode.H,
            KeyCode.I,
            KeyCode.J,
            KeyCode.K,
            KeyCode.L,
            KeyCode.M,
            KeyCode.N,
            KeyCode.O,
            KeyCode.P,
            KeyCode.Q,
            KeyCode.R,
            KeyCode.S,
            KeyCode.T,
            KeyCode.U,
            KeyCode.V,
            KeyCode.W,
            KeyCode.X,
            KeyCode.Y,
            KeyCode.Z,
            KeyCode.At,
            KeyCode.Alpha0,
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.AltGr,
            KeyCode.RightAlt,
            KeyCode.RightApple,
            KeyCode.RightArrow,
            KeyCode.RightBracket,
            KeyCode.RightCommand,
            KeyCode.RightControl,
            KeyCode.RightCurlyBracket,
            KeyCode.RightParen,
            KeyCode.RightShift,
            KeyCode.RightWindows,
            KeyCode.LeftAlt,
            KeyCode.LeftApple,
            KeyCode.LeftArrow,
            KeyCode.LeftBracket,
            KeyCode.LeftCommand,
            KeyCode.LeftControl,
            KeyCode.LeftCurlyBracket,
            KeyCode.LeftParen,
            KeyCode.LeftShift,
            KeyCode.LeftWindows,
            KeyCode.Less,
            KeyCode.Greater,
            KeyCode.Colon,
            KeyCode.Semicolon,
            KeyCode.Break,
            KeyCode.BackQuote,
            KeyCode.Ampersand,
            KeyCode.CapsLock,
            KeyCode.Caret,
            KeyCode.Clear,
            KeyCode.Comma,
            KeyCode.Question,
            KeyCode.Quote,
            KeyCode.End,
            KeyCode.Escape,
            KeyCode.Exclaim,
            KeyCode.UpArrow,
            KeyCode.DownArrow,
            KeyCode.PageUp,
            KeyCode.PageDown,
            KeyCode.Pause,
            KeyCode.Percent,
            KeyCode.Pipe,
            KeyCode.Print,
            KeyCode.SysReq,
            KeyCode.Tab,
            KeyCode.Tilde,
            KeyCode.Underscore,
            KeyCode.Numlock,
            KeyCode.Menu,
            KeyCode.Insert,
            KeyCode.Hash,
            KeyCode.Help,
            KeyCode.Home,
            KeyCode.Delete,
            KeyCode.Dollar,
            KeyCode.Space,
            KeyCode.ScrollLock,
            KeyCode.Backspace,
            KeyCode.Slash,
            KeyCode.Backslash,
            KeyCode.Return,
            KeyCode.Equals,
            KeyCode.Minus,
            KeyCode.Asterisk,
            KeyCode.Period,
            KeyCode.Plus,
            KeyCode.Keypad0,
            KeyCode.Keypad1,
            KeyCode.Keypad2,
            KeyCode.Keypad3,
            KeyCode.Keypad4,
            KeyCode.Keypad5,
            KeyCode.Keypad6,
            KeyCode.Keypad7,
            KeyCode.Keypad8,
            KeyCode.Keypad9,
            KeyCode.KeypadDivide,
            KeyCode.KeypadEnter,
            KeyCode.KeypadEquals,
            KeyCode.KeypadMinus,
            KeyCode.KeypadMultiply,
            KeyCode.KeypadPeriod,
            KeyCode.KeypadPlus,
            KeyCode.F1,
            KeyCode.F2,
            KeyCode.F3,
            KeyCode.F4,
            KeyCode.F5,
            KeyCode.F6,
            KeyCode.F7,
            KeyCode.F8,
            KeyCode.F9,
            KeyCode.F10,
            KeyCode.F11,
            KeyCode.F12,
        };

        public static bool IsMousePos(UserInputType userInputType)
        {
            if (userInputType == UserInputType.MOUSEBUTTON1 || userInputType == UserInputType.MOUSEBUTTON2 || userInputType == UserInputType.MOUSEBUTTON3)
                return true;
            return false;
        }

        public static UserInputType TypeFromKey(KeyCode keyCode)
        {
            string KeyString = keyCode.ToString();
            if (keyCode == KeyCode.Mouse0)
                return UserInputType.MOUSEBUTTON1;
            else if (keyCode == KeyCode.Mouse1)
                return UserInputType.MOUSEBUTTON2;
            else if (keyCode == KeyCode.Mouse2)
                return UserInputType.MOUSEBUTTON3;
            else if (KeyString.StartsWith("Mouse"))
                return UserInputType.MOUSE;
            else if (KeyString.StartsWith("Joystick1"))
                return UserInputType.GAMEPAD1;
            else if (KeyString.StartsWith("Joystick2"))
                return UserInputType.GAMEPAD2;
            else if (KeyString.StartsWith("Joystick3"))
                return UserInputType.GAMEPAD3;
            else if (KeyString.StartsWith("Joystick4"))
                return UserInputType.GAMEPAD4;
            else if (KeyString.StartsWith("Joystick5"))
                return UserInputType.GAMEPAD5;
            else if (KeyString.StartsWith("Joystick6"))
                return UserInputType.GAMEPAD6;
            else if (KeyString.StartsWith("Joystick7"))
                return UserInputType.GAMEPAD7;
            else if (KeyString.StartsWith("Joystick8"))
                return UserInputType.GAMEPAD8;
            else if (KeyString.StartsWith("Joystick"))
                return UserInputType.GAMEPAD;
            else if (KeyString.StartsWith("Alpha") || KeyString.StartsWith("Keypad") || KeyboardCodes.Contains(keyCode))
                return UserInputType.KEYBOARD;
            else
                return TypeFromRunning();
        }
        public static bool IsKeyboard()
        {
            return (IsKeyboardPlayer() || IsKeyboardEditor() || IsStoreApp());
        }

        public static bool IsKeyboardPlayer()
        {
            return (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.OSXPlayer);
        }

        public static bool IsStoreApp()
        {
            return (Application.platform == RuntimePlatform.WSAPlayerARM || Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerX86);
        }

        public static bool IsKeyboardEditor()
        {
            return (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.OSXEditor);
        }

        public static bool IsConsole()
        {
            return (Application.platform == RuntimePlatform.XboxOne || Application.platform == RuntimePlatform.Switch || Application.platform == RuntimePlatform.PS4);
        }

        public static bool IsTouch()
        {
            return (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer);
        }

        public static bool IsVR()
        {
            return false;
        }

        public static bool IsEditor()
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }

        public static UserInputType TypeFromRunning()
        {
            if (IsVR())
                return UserInputType.VR;
            else if (IsKeyboard())
                return UserInputType.KEYBOARD;
            else if (IsConsole())
                return UserInputType.GAMEPAD;
            else if (IsTouch())
                return UserInputType.TOUCH;
            else
                return UserInputType.NONE;
        }

        public enum UserInputType
        {
            NONE,
            GAMEPAD,
            GAMEPAD1,
            GAMEPAD2,
            GAMEPAD3,
            GAMEPAD4,
            GAMEPAD5,
            GAMEPAD6,
            GAMEPAD7,
            GAMEPAD8,
            KEYBOARD,
            MOUSEBUTTON1,
            MOUSEBUTTON2,
            MOUSEBUTTON3,
            MOUSEMOVEMENT,
            MOUSEWHEEL,
            MOUSE,
            TEXTINPUT,
            TOUCH,
            VR,
        }

        public class InputObject
        {
            public InputObject(Vector2 pos, UserInputType inputType = UserInputType.NONE, KeyCode keyCode = KeyCode.None)
            {
                this.keyCode = keyCode;
                this.inputType = inputType != UserInputType.NONE ? inputType : TypeFromKey(keyCode);
                this.pos = pos;
            }

            public UserInputType inputType { get; private set; }
            public KeyCode keyCode { get; private set; }
            public Vector2 pos { get; private set; }
        }

        public event System.Action<InputObject, bool> InputBegan;
        public event System.Action<InputObject, bool> InputChanged;
        public event System.Action<InputObject, bool> InputEnded;
        public event System.Action<InputObject, bool> TouchStarted;
        public event System.Action<InputObject, bool> TouchEnded;

        private void RunInInputDetector(System.Action<InputDetector> action)
        {
            foreach (InputDetector inputDetector in UnityObjectUtils.GetObjectsOfInterface<InputDetector>().Keys)
            {
                action(inputDetector);
            }
        }

        private void RunInTouchDetector(System.Action<TouchDetector> action)
        {
            foreach (TouchDetector touchDetector in UnityObjectUtils.GetObjectsOfInterface<TouchDetector>().Keys)
            {
                action(touchDetector);
            }
        }

        internal void OnInputBegan(InputObject io, bool wp)
        {
            InputBegan?.Invoke(io, wp);
            RunInInputDetector((d => d?.InputBegan(io, wp)));
        }

        internal void OnInputChanged(InputObject io, bool wp)
        {
            InputChanged?.Invoke(io, wp);
            RunInInputDetector((d => d?.InputChanged(io, wp)));
        }

        internal void OnInputEnded(InputObject io, bool wp)
        {
            InputEnded?.Invoke(io, wp);
            RunInInputDetector((d => d?.InputEnded(io, wp)));
        }

        internal void OnTouchStarted(InputObject io, bool wp)
        {
            TouchStarted?.Invoke(io, wp);
            RunInTouchDetector((d => d?.TouchStarted(io, wp)));
        }

        internal void OnTouchEnded(InputObject io, bool wp)
        {
            TouchEnded?.Invoke(io, wp);
            RunInTouchDetector((d => d?.TouchEnded(io, wp)));
        }

        private List<KeyCode> KeysDown = new List<KeyCode>();

        public static bool IsKeyDown(KeyCode key) => Instance.KeysDown.Contains(key);

        public static bool IsKeyUp(KeyCode key) => !IsKeyDown(key);

        protected override void Awake()
        {
            base.Awake();
            Platform = Application.platform.ToString().ToUpper();
            KeysDown = new List<KeyCode>();
            lastInputType = TypeFromRunning();
        }

        public void Update()
        {
            Cursor.visible = MouseVisible;
            float oldX = this.mousePos.x;
            float oldY = this.mousePos.y;
            float newX = Input.mousePosition.x;
            float newY = (float)Screen.height - Input.mousePosition.y;
            this.mousePos.x = newX;
            this.mousePos.y = newY;
            if (newX != oldX || newY != oldY)
            {
                OnInputChanged(new InputObject(this.mousePos, UserInputType.MOUSEMOVEMENT, KeyCode.None), false);
            }
            foreach (KeyCode vKey in EnumUtils.GetAll<KeyCode>())
            {
                if (KeysDown.Contains(vKey))
                {
                    if ((!Input.GetKeyDown(vKey) && !Input.GetKey(vKey)) || Input.GetKeyUp(vKey))
                    {
                        UserInputType currentInputType = TypeFromKey(vKey);
                        lastInputType = currentInputType;
                        KeysDown.Remove(vKey);
                        Vector2 pos = IsMousePos(currentInputType) ? this.mousePos : Vector2.zero;
                        OnInputEnded(new InputObject(pos, currentInputType, vKey), false);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(vKey) || Input.GetKey(vKey))
                    {
                        UserInputType currentInputType = TypeFromKey(vKey);
                        lastInputType = currentInputType;
                        KeysDown.Add(vKey);
                        Vector2 pos = IsMousePos(currentInputType) ? this.mousePos : Vector2.zero;
                        OnInputBegan(new InputObject(pos, currentInputType, vKey), false);
                    }
                }
            }
        }

        #region Get Number Pressed

        /// <summary>
        /// Is number key pressed (Numpad included)
        /// </summary>
        public static bool GetNumberDown(int num)
        {
            Debug.Assert(num <= 9 && num >= 0);

            switch (num)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) return true;
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return true;
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return true;
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return true;
                    break;
                case 4:
                    if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return true;
                    break;
                case 5:
                    if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return true;
                    break;
                case 6:
                    if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) return true;
                    break;
                case 7:
                    if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) return true;
                    break;
                case 8:
                    if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) return true;
                    break;
                case 9:
                    if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) return true;
                    break;
                default:
                    break;
            }

            return false;
        }

        /// <summary>
        /// Is KeyCode is number (Numpad included)
        /// </summary>
        /// <returns>If KeyCode is not a number returns -1</returns>
        public static int GetNumberDown(KeyCode key)
        {
            if (key == KeyCode.Alpha0 || key == KeyCode.Keypad0) return 0;
            if (key == KeyCode.Alpha1 || key == KeyCode.Keypad1) return 1;
            if (key == KeyCode.Alpha2 || key == KeyCode.Keypad2) return 2;
            if (key == KeyCode.Alpha3 || key == KeyCode.Keypad3) return 3;
            if (key == KeyCode.Alpha4 || key == KeyCode.Keypad4) return 4;
            if (key == KeyCode.Alpha5 || key == KeyCode.Keypad5) return 5;
            if (key == KeyCode.Alpha6 || key == KeyCode.Keypad6) return 6;
            if (key == KeyCode.Alpha7 || key == KeyCode.Keypad7) return 7;
            if (key == KeyCode.Alpha8 || key == KeyCode.Keypad8) return 8;
            if (key == KeyCode.Alpha9 || key == KeyCode.Keypad9) return 9;

            return -1;
        }

        /// <summary>
        /// Is Input.GetKeyDown is number (Numpad included)
        /// </summary>
        /// <returns>If none pressed returns -1</returns>
        public static int GetNumberDown()
        {
            if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) return 0;
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) return 1;
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) return 2;
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) return 3;
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) return 4;
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) return 5;
            if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) return 6;
            if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) return 7;
            if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) return 8;
            if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) return 9;

            return -1;
        }

        #endregion

        /// <summary>
        /// key1 or key2 is pressed
        /// </summary>
        public static bool AnyKeyDown(KeyCode key1, KeyCode key2)
        {
            return Input.GetKeyDown(key1) || Input.GetKeyDown(key2);
        }

        /// <summary>
        /// key1, key2 or key3 is pressed
        /// </summary>
        public static bool AnyKeyDown(KeyCode key1, KeyCode key2, KeyCode key3)
        {
            return AnyKeyDown(key1, key2) || Input.GetKeyDown(key3);
        }


        /// <summary>
        /// "A", "Left Arrow" and "Numpad 4"
        /// </summary>
        public static bool IsLeft()
        {
            return AnyKeyDown(KeyCode.A, KeyCode.LeftArrow, KeyCode.Keypad4);
        }

        /// <summary>
        /// "D", "Right Arrow" and "Numpad 6"
        /// </summary>
        public static bool IsRight()
        {
            return AnyKeyDown(KeyCode.D, KeyCode.RightArrow, KeyCode.Keypad6);
        }

        /// <summary>
        /// "W", "Up Arrow" and "Numpad 8"
        /// </summary>
        public static bool IsUp()
        {
            return AnyKeyDown(KeyCode.W, KeyCode.UpArrow, KeyCode.Keypad8);
        }

        /// <summary>
        /// "S", "Down Arrow" and "Numpad 2"
        /// </summary>
        public static bool IsDown()
        {
            return AnyKeyDown(KeyCode.S, KeyCode.DownArrow, KeyCode.Keypad2);
        }

        /// <summary>
        /// Roguelike movement input, where top-left is 7 and bottom-right is 3 
        /// </summary>
        public static int KeypadDirection()
        {
            if (IsLeft()) return 4;
            if (IsRight()) return 6;
            if (IsUp()) return 8;
            if (IsDown()) return 2;

            if (Input.GetKeyDown(KeyCode.Keypad1)) return 1;
            if (Input.GetKeyDown(KeyCode.Keypad3)) return 3;
            if (Input.GetKeyDown(KeyCode.Keypad7)) return 7;
            if (Input.GetKeyDown(KeyCode.Keypad9)) return 9;

            return 0;
        }

        /// <summary>
        /// Roguelike movement input on X axis
        /// </summary>
        /// <returns>1 if moved to  right/bottom-right/top-right, -1 if moved to left/bottom-left/top-left, </returns>
        public static int KeypadX()
        {
            if (IsLeft()) return -1;
            if (IsRight()) return 1;
            if (Input.GetKeyDown(KeyCode.Keypad1)) return -1;
            if (Input.GetKeyDown(KeyCode.Keypad7)) return -1;
            if (Input.GetKeyDown(KeyCode.Keypad3)) return 1;
            if (Input.GetKeyDown(KeyCode.Keypad9)) return 1;

            return 0;
        }

        /// <summary>
        /// Roguelike movement input on Y axis
        /// </summary>
        /// <returns>1 if moved to top/top-left/top-right, -1 if moved to bottom/bottom-left/bottom-right</returns>
        public static int KeypadY()
        {
            if (IsUp()) return 1;
            if (IsDown()) return -1;
            if (Input.GetKeyDown(KeyCode.Keypad1)) return -1;
            if (Input.GetKeyDown(KeyCode.Keypad3)) return -1;

            if (Input.GetKeyDown(KeyCode.Keypad7)) return 1;
            if (Input.GetKeyDown(KeyCode.Keypad9)) return 1;

            return 0;
        }
    }

    //[HarmonyPatch(typeof(MainScript))]
    //[HarmonyPatch("Start")]
    //internal class InputInject
    //{
    //    [HarmonyPriority(Priority.First)]
    //    public static void Prefix(MainScript __instance)
    //    {
    //        if (!__instance.HasComponent<UserInputService>())
    //            __instance.AddComponent<UserInputService>();
    //    }

    //    [HarmonyPriority(Priority.First)]
    //    public static void Postfix(MainScript __instance)
    //    {
    //        SALT.Console.KeyBindManager.ReadBinds();
    //        if (!__instance.HasComponent<SALT.Console.KeyBindManager.ProcessAllBindings>())
    //            __instance.AddComponent<SALT.Console.KeyBindManager.ProcessAllBindings>();
    //    }
    //}
}
