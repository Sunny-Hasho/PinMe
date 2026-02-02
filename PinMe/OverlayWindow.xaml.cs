using System;
using System.Windows;
using System.Windows.Interop;

namespace PinWin
{
    public partial class OverlayWindow : Window
    {
        public OverlayWindow()
        {
            InitializeComponent();
        }

        public IntPtr Handle { get; private set; }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.Handle = new WindowInteropHelper(this).Handle;
            
            // Set styles: Transparent (Click-through) | ToolWindow (Hide from Alt-Tab)
            int extendedStyle = PinWin.Interop.Win32.GetWindowLongPtr(this.Handle, PinWin.Interop.Win32.GWL_EXSTYLE).ToInt32();
            PinWin.Interop.Win32.SetWindowLong(this.Handle, PinWin.Interop.Win32.GWL_EXSTYLE, extendedStyle | (int)PinWin.Interop.Win32.WS_EX_TRANSPARENT | (int)PinWin.Interop.Win32.WS_EX_TOOLWINDOW);

            PinWin.Interop.Win32.SetWindowPos(this.Handle, PinWin.Interop.Win32.HWND_TOPMOST, 0, 0, 0, 0, PinWin.Interop.Win32.SWP_NOMOVE | PinWin.Interop.Win32.SWP_NOSIZE | PinWin.Interop.Win32.SWP_SHOWWINDOW);
        }
    }
}
