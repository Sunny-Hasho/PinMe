using System;
using System.Collections.Generic;
using System.Windows.Threading;
using PinWin.Interop;
using System.Linq;

namespace PinWin.Services
{
    public class OverlayService : IDisposable
    {
        private Dictionary<IntPtr, OverlayWindow> _overlays = new Dictionary<IntPtr, OverlayWindow>();
        private DispatcherTimer _trackingTimer;

        public OverlayService()
        {
            _trackingTimer = new DispatcherTimer();
            _trackingTimer.Interval = TimeSpan.FromMilliseconds(10);
            _trackingTimer.Tick += TrackingTimer_Tick;
            _trackingTimer.Start();
        }

        public IntPtr AddOverlay(IntPtr targetHwnd)
        {
            if (_overlays.ContainsKey(targetHwnd))
            {
                Logger.Log($"OverlayService: Already tracking {targetHwnd}");
                return _overlays[targetHwnd].Handle;
            }

            var overlay = new OverlayWindow();
            overlay.Show();
            _overlays.Add(targetHwnd, overlay);
            Logger.Log($"OverlayService: Added overlay for {targetHwnd}");

            // Note: We do NOT set Owner here anymore.
            // WindowPinService handles the complex "Zipper" chaining.
            
            // Immediate update
            UpdateOverlayPosition(targetHwnd, overlay);
            
            return overlay.Handle;
        }

        public void RemoveOverlay(IntPtr targetHwnd)
        {
            if (_overlays.ContainsKey(targetHwnd))
            {
                var overlay = _overlays[targetHwnd];
                overlay.Close();
                _overlays.Remove(targetHwnd);
                Logger.Log($"OverlayService: Removed overlay for {targetHwnd}");
            }
            else 
            {
                Logger.Log($"OverlayService: Request to remove known {targetHwnd} but not found in dict");
            }
        }

        private void TrackingTimer_Tick(object sender, EventArgs e)
        {
            // ToList to allow modification (removal) during iteration if needed, though we mainly remove on "close" detection
            var keys = _overlays.Keys.ToList();
            foreach (var hwnd in keys)
            {
                if (_overlays.TryGetValue(hwnd, out var overlay))
                {
                    UpdateOverlayPosition(hwnd, overlay);
                }
            }
        }

        private void UpdateOverlayPosition(IntPtr hwnd, OverlayWindow overlay)
        {
            Win32.RECT rect;
            // Try getting visual bounds first (Windows Vista+)
            int result = Win32.DwmGetWindowAttribute(hwnd, Win32.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, System.Runtime.InteropServices.Marshal.SizeOf(typeof(Win32.RECT)));
            
            // Fallback to GetWindowRect if DWM fails (rare on modern Windows, but safer)
            if (result != 0)
            {
                 // If DWM fails, it might be a window that doesn't support it or is closing.
                 // We specifically AVOID GetWindowRect here because it includes shadows/invisible borders
                 // which causes the "size change" flickering/jumping the user reported.
                 
                 // Just check if window is still valid to decide if we should close overlay
                 Win32.RECT temp;
                 if (!Win32.GetWindowRect(hwnd, out temp))
                 {
                    // Window definitely closed/invalid
                    RemoveOverlay(hwnd);
                    return;
                 }
                 
                 // If valid but DWM failed, skip this update to avoid jitter
                 return;
            }

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

                if (width <= 0 || height <= 0)
                {
                     // Minimized or hidden, hide overlay
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                    return;
                }

                // 1. Check Basic Visibility
                if (!Win32.IsWindowVisible(hwnd))
                {
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                    return;
                }

                // 2. Check "Cloaked" State (UWP/Virtual Desktop)
                // ChatGPT and other modern apps often "Close" by becoming Cloaked.
                int cloakedVal;
                Win32.DwmGetWindowAttribute(hwnd, Win32.DWMWA_CLOAKED, out cloakedVal, sizeof(int));
                if (cloakedVal != 0)
                {
                    overlay.Visibility = System.Windows.Visibility.Hidden;
                    return;
                }

                if (overlay.Visibility != System.Windows.Visibility.Visible)
                {
                    overlay.Visibility = System.Windows.Visibility.Visible;
                }

                // Direct Win32 positioning (Physical pixels) - Bypasses WPF Layout & DPI conversion issues
                // Use SWP_NOZORDER to ensure we don't force the overlay to the top of the stack every 10ms.
                // The Z-order is handled by the Owner relationship (GWLP_HWNDPARENT) established in AddOverlay.
                Win32.SetWindowPos(overlay.Handle, IntPtr.Zero, 
                    rect.Left, rect.Top, width, height, 
                    Win32.SWP_NOZORDER | Win32.SWP_NOACTIVATE | Win32.SWP_SHOWWINDOW);
            }

        public void Dispose()
        {
            _trackingTimer.Stop();
            foreach (var overlay in _overlays.Values)
            {
                overlay.Close();
            }
            _overlays.Clear();
        }
    }
}
