using System;
using System.Runtime.InteropServices;
using Pinnit.Interop;

namespace Pinnit.Services
{
    public class HotkeyService
    {
        private const int ID_HOTKEY = 9000;
        public event EventHandler HotkeyPressed;

        public bool Register(IntPtr hWnd)
        {
            // Ctrl + Win + T
            // T = 0x54
            return Win32.RegisterHotKey(hWnd, ID_HOTKEY, Win32.MOD_CONTROL | Win32.MOD_WIN, 0x54);
        }

        public void Unregister(IntPtr hWnd)
        {
            Win32.UnregisterHotKey(hWnd, ID_HOTKEY);
        }

        public void ProcessMessage(int msg, IntPtr wParam)
        {
            if (msg == Win32.WM_HOTKEY && wParam.ToInt32() == ID_HOTKEY)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
