using HeroVirtualTabletop.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace HeroVirtualTabletop.Desktop
{
    public delegate void EventMethod();
    public delegate EventMethod HandleKeyEvent(Keys vkCode, System.Windows.Input.Key inputKey);
    public interface DesktopKeyEventHandler
    {
        void AddKeyEventHandler(HandleKeyEvent handleKeyEvent);
        void RemoveKeyEventHandler(HandleKeyEvent handleKeyEvent);
        void SuspendKeyEventHandlersExcept(params HandleKeyEvent[] excludedHandleKeyEvents);
        void ResumeKeyEventHandlersExcept(params HandleKeyEvent[] excludedHandleKeyEvents);
        void SuspendAllKeyEventHandlers();
        void ResumeAllKeyEventHandlers();

    }
    public class DesktopKeyEventHandlerImpl : DesktopKeyEventHandler
    {
        private object lockObj = new object();
        private IntPtr hookID;
        private Keys vkCode; 
        private List<HandleKeyEvent> _handleKeyEvents;
        private List<HandleKeyEvent> _suspendedHandleKeyEvents;
        public DesktopKeyEventHandlerImpl()
        {
            _handleKeyEvents = new List<Desktop.HandleKeyEvent>();
            _suspendedHandleKeyEvents = new List<Desktop.HandleKeyEvent>();
            ActivateKeyboardHook();
        }

        public void ActivateKeyboardHook()
        {

            hookID = KeyBoardHook.SetHook(this.HandleKeyboardEvent);
        }

        public void AddKeyEventHandler(HandleKeyEvent handleKeyEvent)
        {
            lock (lockObj)
            {
                if (!_handleKeyEvents.Contains(handleKeyEvent))
                {
                    _handleKeyEvents.Add(handleKeyEvent);
                }
            }
        }

        public void RemoveKeyEventHandler(HandleKeyEvent handleKeyEvent)
        {
            lock (lockObj)
            {
                if (_handleKeyEvents.Contains(handleKeyEvent))
                {
                    _handleKeyEvents.Remove(handleKeyEvent);
                }
            }
        }

        public void SuspendKeyEventHandlersExcept(params HandleKeyEvent[] excludedHandleKeyEvents)
        {
            foreach (var keyEvent in _handleKeyEvents.ToList())
            {
                if (!excludedHandleKeyEvents.Contains(keyEvent) && !_suspendedHandleKeyEvents.Contains(keyEvent))
                    _suspendedHandleKeyEvents.Add(keyEvent);
            }
        }
        public void ResumeKeyEventHandlersExcept(params HandleKeyEvent[] excludedHandleKeyEvents)
        {
            foreach(var keyEvent in _suspendedHandleKeyEvents.ToList())
            {
                if (!excludedHandleKeyEvents.Contains(keyEvent) && _suspendedHandleKeyEvents.Contains(keyEvent))
                    _suspendedHandleKeyEvents.Remove(keyEvent);
            }
        }
        public void SuspendAllKeyEventHandlers()
        {
            _suspendedHandleKeyEvents.AddRange(_handleKeyEvents);
        }
        public void ResumeAllKeyEventHandlers()
        {
            _suspendedHandleKeyEvents.Clear();
        }

        internal void DeactivateKeyboardHook()
        {
            KeyBoardHook.UnsetHook(hookID);
        }
        internal IntPtr CallNextHook(IntPtr hookID, int nCode, IntPtr wParam, IntPtr lParam)
        {
            return KeyBoardHook.CallNextHookEx(hookID, nCode, wParam, lParam);
        }

        internal System.Windows.Input.Key InputKey
        {
            get
            {
                return KeyInterop.KeyFromVirtualKey((int)this.vkCode);
            }
        }
        internal Boolean ApplicationIsActiveWindow
        {
            get
            {
                uint wndProcId;
                IntPtr foregroundWindow = WindowsUtilities.GetForegroundWindow();
                uint wndProcThread = WindowsUtilities.GetWindowThreadProcessId(foregroundWindow, out wndProcId);
                var currentProcId = Process.GetCurrentProcess().Id;
                return currentProcId == wndProcId;
            }
        }

        internal IntPtr HandleKeyboardEvent(int nCode, IntPtr wParam, IntPtr lParam)
        {

            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT keyboardLLHookStruct = (KBDLLHOOKSTRUCT)(Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT)));
                this.vkCode = (Keys)keyboardLLHookStruct.vkCode;
                KeyboardMessage wmKeyboard = (KeyboardMessage)wParam;
                if ((wmKeyboard == KeyboardMessage.WM_KEYDOWN || wmKeyboard == KeyboardMessage.WM_SYSKEYDOWN))
                {
                    IntPtr foregroundWindow = WindowsUtilities.GetForegroundWindow();
                    uint wndProcId;
                    uint wndProcThread = WindowsUtilities.GetWindowThreadProcessId(foregroundWindow, out wndProcId);
                    if (foregroundWindow == WindowsUtilities.FindWindow("CrypticWindow", null)
                        || Process.GetCurrentProcess().Id == wndProcId)
                    {
                        System.Windows.Input.Key inputKey = InputKey;
                        if ((inputKey == Key.Left || inputKey == Key.Right) && Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            DesktopManager.SetFocusToDesktop();
                        }
                        else
                        {
                            lock (lockObj)
                            {
                                try
                                {
                                    foreach (HandleKeyEvent _handleKeyEvent in _handleKeyEvents.Where(handler => !_suspendedHandleKeyEvents.Contains(handler)))
                                    {
                                        EventMethod handler = _handleKeyEvent(vkCode, inputKey);
                                        if (handler != null)
                                        {
                                            handler();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                }
            }
            return CallNextHook(hookID, nCode, wParam, lParam);
        }
    }
    public static class KeyBoardHook
    {
        #region Imports

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        #endregion

        static KeyBoardHook()
        {
            AppDomain.CurrentDomain.ProcessExit += UnsetHooks;
        }
        private static Dictionary<IntPtr, LowLevelKeyboardProc> hookedProcs = new Dictionary<IntPtr, LowLevelKeyboardProc>();
        private static List<IntPtr> hookedProcIDs = new List<IntPtr>();
        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                IntPtr hookID = SetWindowsHookEx((int)HookType.WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
                hookedProcIDs.Add(hookID);
                hookedProcs.Add(hookID, proc);
                return hookID;
            }
        }

        public static void UnsetHook(IntPtr hookID)
        {
            if (hookedProcIDs.Contains(hookID))
            {
                UnhookWindowsHookEx(hookID);
                hookedProcIDs.Remove(hookID);
                hookedProcs.Remove(hookID);
            }
        }

        private static void UnsetHooks(object sender, EventArgs e)
        {
            foreach (IntPtr hookID in hookedProcIDs)
            {
                UnhookWindowsHookEx(hookID);
            }
        }
    }
    /// <summary>
    /// The structure contains information about a low-level keyboard input event. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct KBDLLHOOKSTRUCT
    {
        public int vkCode;      // Specifies a virtual-key code
        public int scanCode;    // Specifies a hardware scan code for the key
        public int flags;
        public int time;        // Specifies the time stamp for this message
        public int dwExtraInfo;
    }

    public enum KeyboardMessage
    {
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_SYSKEYDOWN = 0x0104,
        WM_SYSKEYUP = 0x0105
    }

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
}
