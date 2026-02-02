using System;
using System.Windows.Forms;
using PinWin.Interop;

namespace PinWin.Services
{
    public class TrayService : IDisposable
    {
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        
        // Changed to use IntPtr for specific window
        public event EventHandler<IntPtr> PinWindowRequested;
        public event EventHandler UnpinRequested; // Keep generic unpin active for now or use same event?
        public event EventHandler ExitRequested;

        public void Initialize()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Opening += ContextMenu_Opening;

            _contextMenu.Items.Add("Pin Active Window (Hotkey preferred)", null, (s, e) => PinWindowRequested?.Invoke(this, Win32.GetForegroundWindow())); // Still broken for tray clicks usually
            _contextMenu.Items.Add("-");
            // Dynamic list will be added here
            _contextMenu.Items.Add("Exit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));

            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information, // Placeholder
                Visible = true,
                Text = "PinWin",
                ContextMenuStrip = _contextMenu
            };
        }

        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Rebuild menu on open
            _contextMenu.Items.Clear();
            
            // "Pin Window" Submenu
            var pinMenu = new ToolStripMenuItem("Pin Window...");
            PopulateWindowList(pinMenu);
            _contextMenu.Items.Add(pinMenu);

            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("Exit", null, (s, e) => ExitRequested?.Invoke(this, EventArgs.Empty));
        }

        private void PopulateWindowList(ToolStripMenuItem parent)
        {
            Win32.EnumWindows((hwnd, lParam) =>
            {
                if (Win32.IsWindowVisible(hwnd))
                {
                    int length = Win32.GetWindowTextLength(hwnd);
                    if (length > 0)
                    {
                        var sb = new System.Text.StringBuilder(length + 1);
                        Win32.GetWindowText(hwnd, sb, sb.Capacity);
                        string title = sb.ToString();

                        // Filter out empty or self
                        if (!string.IsNullOrWhiteSpace(title) && title != "PinWin" && title != "Program Manager")
                        {
                            parent.DropDownItems.Add(title, null, (s, e) => PinWindowRequested?.Invoke(this, hwnd));
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);
            
            if (parent.DropDownItems.Count == 0)
            {
                parent.DropDownItems.Add("(No visible windows found)");
            }
        }

        public void Dispose()
        {
            _notifyIcon?.Dispose();
            _contextMenu?.Dispose();
        }
    }
}
