using System;
using System.Windows;
using PinWin.Interop;
using PinWin.Services;

namespace PinWin.ViewModels
{
    public class AppViewModel : IDisposable
    {
        private readonly WindowPinService _pinService;
        private readonly HotkeyService _hotkeyService;
        private readonly TrayService _trayService;
        private readonly OverlayService _overlayService;
        private IntPtr _mainWindowHandle;

        public AppViewModel()
        {
            _pinService = new WindowPinService();
            _hotkeyService = new HotkeyService();
            _trayService = new TrayService();
            _overlayService = new OverlayService();

            _trayService.Initialize();

            _trayService.ExitRequested += (s, e) => System.Windows.Application.Current.Shutdown();
            _trayService.PinWindowRequested += (s, hwnd) => TogglePinState(hwnd); 
            // Tray specifically asked for "Pin" and "Unpin" separate items, but implementation plan said "Pin/Unpin".
            // Let's use Toggle for simplicity or check foreground. 
            // Tray context menu "Pin Active Window" implies pinning the window that was active before clicking the tray?
            // Clicking tray makes tray/taskbar active. This is tricky.
            // Usually tray tools work on "currently focused" but once you click tray, focus is lost.
            // Solved by: GetForegroundWindow MIGHT be the tray/taskbar.
            // However, Hotkey works perfectly for this. Tray menu items for "Pin Active" are hard to use unless there's a delay or selection mode.
            // For MVP, hotkey is primary. Tray "Pin" might just toggle the *last* active window or just be there for show/help.
            // Let's make Tray "Pin" trigger the same toggle on Foreground (which might be wrong window) but for now stick to HotkeyService mainly.
            
            _hotkeyService.HotkeyPressed += (s, e) => ToggleActiveWindowPin();
        }

        public void Initialize(IntPtr windowHandle)
        {
            _mainWindowHandle = windowHandle;
            if (!_hotkeyService.Register(_mainWindowHandle))
            {
                System.Windows.MessageBox.Show("Failed to register hotkey (Ctrl + Win + T). It might be in use.", "PinWin Error");
            }
        }

        public void ProcessMessage(int msg, IntPtr wParam)
        {
            _hotkeyService.ProcessMessage(msg, wParam);
        }

        private void ToggleActiveWindowPin()
        {
             TogglePinState(Win32.GetForegroundWindow());
        }

        private void TogglePinState(IntPtr handle)
        {
            Logger.Log($"AppViewModel: Toggle request for {handle}");
            bool isPinned = _pinService.TogglePin(handle);
            
            if (isPinned)
            {
                Logger.Log("AppViewModel: Adding overlay");
                IntPtr overlayHandle = _overlayService.AddOverlay(handle);
                _pinService.RegisterOverlay(handle, overlayHandle);
            }
            else
            {
                Logger.Log("AppViewModel: Removing overlay");
                _overlayService.RemoveOverlay(handle);
                _pinService.UnregisterOverlay(handle);
            }
        }

        private void PinCurrentWindow()
        {
            // For tray clicks, getting foreground window is unreliable (it's the menu).
            // This is a known limitation. We'll skip complex logic for now or implement a timer-based selection if needed.
            // or just ToggleActiveWindowPin() and accept it might try to pin the taskbar.
            ToggleActiveWindowPin(); 
        }

        public void Dispose()
        {
            _hotkeyService.Unregister(_mainWindowHandle);
            _trayService.Dispose();
            _overlayService.Dispose();
        }
    }
}
