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
        public event EventHandler<bool> ShowPetIconChanged;
        public event EventHandler<bool> ShowBorderChanged;
        public event EventHandler<int> BorderThicknessChanged;
        public event EventHandler<System.Windows.Media.Brush> BorderColorChanged;
        public event EventHandler<string> PetIconChanged;
        public event EventHandler<int> PetIconSizeChanged;
        
        public bool ShowPetIcon { get; set; } = true;
        public bool ShowBorder { get; set; } = true;

        public void Initialize()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Opening += ContextMenu_Opening;

            // Initial build isn't strictly necessary as Opening handles it, but good for safety
            _contextMenu.Items.Add("Pin Active Window (Hotkey preferred)", null, (s, e) => PinWindowRequested?.Invoke(this, Win32.GetForegroundWindow())); 
            _contextMenu.Items.Add("-");
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

            // Pet Icon Toggle
            var petItem = new ToolStripMenuItem("Show Pet Icon");
            petItem.Checked = ShowPetIcon;
            petItem.Click += (s, args) => 
            {
                ShowPetIcon = !ShowPetIcon;
                ShowPetIconChanged?.Invoke(this, ShowPetIcon);
            };
            _contextMenu.Items.Add(petItem);

            var changeIconItem = new ToolStripMenuItem("Change Icon...");
            changeIconItem.Click += (s, args) =>
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All Files|*.*";
                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        PetIconChanged?.Invoke(this, openFileDialog.FileName);
                    }
                }
            };
            _contextMenu.Items.Add(changeIconItem);

            // Icon Size Submenu
            var sizeMenu = new ToolStripMenuItem("Icon Size");
            
            var smallSize = new ToolStripMenuItem("Small (30px)");
            smallSize.Click += (s, args) => PetIconSizeChanged?.Invoke(this, 30);
            
            var mediumSize = new ToolStripMenuItem("Medium (50px)");
            mediumSize.Click += (s, args) => PetIconSizeChanged?.Invoke(this, 50);

            var largeSize = new ToolStripMenuItem("Large (80px)");
            largeSize.Click += (s, args) => PetIconSizeChanged?.Invoke(this, 80);

            sizeMenu.DropDownItems.Add(smallSize);
            sizeMenu.DropDownItems.Add(mediumSize);
            sizeMenu.DropDownItems.Add(largeSize);

            _contextMenu.Items.Add(sizeMenu);

            _contextMenu.Items.Add("-");

            // Border Toggle
            var borderItem = new ToolStripMenuItem("Show Border");
            borderItem.Checked = ShowBorder;
            borderItem.Click += (s, args) =>
            {
                ShowBorder = !ShowBorder;
                ShowBorderChanged?.Invoke(this, ShowBorder);
            };
            _contextMenu.Items.Add(borderItem);

            // Border Thickness Submenu
            var thicknessMenu = new ToolStripMenuItem("Border Thickness");
            
            var thinItem = new ToolStripMenuItem("Thin (2px)");
            thinItem.Click += (s, args) => BorderThicknessChanged?.Invoke(this, 2);
            
            var normalItem = new ToolStripMenuItem("Normal (4px)");
            normalItem.Click += (s, args) => BorderThicknessChanged?.Invoke(this, 4);

            var thickItem = new ToolStripMenuItem("Thick (8px)");
            thickItem.Click += (s, args) => BorderThicknessChanged?.Invoke(this, 8);

            thicknessMenu.DropDownItems.Add(thinItem);
            thicknessMenu.DropDownItems.Add(normalItem);
            thicknessMenu.DropDownItems.Add(thickItem);
            
            _contextMenu.Items.Add(thicknessMenu);

            // Border Color Submenu
            var colorMenu = new ToolStripMenuItem("Border Color");

            void AddColorItem(string name, System.Windows.Media.Brush color)
            {
                var item = new ToolStripMenuItem(name);
                item.Click += (s, args) => BorderColorChanged?.Invoke(this, color);
                colorMenu.DropDownItems.Add(item);
            }

            AddColorItem("White", System.Windows.Media.Brushes.White);
            AddColorItem("Red", System.Windows.Media.Brushes.Red);
            AddColorItem("Green", System.Windows.Media.Brushes.Lime); // Lime is standard "Green" in WPF Brushes usually, or Green. Lime is brighter.
            AddColorItem("Blue", System.Windows.Media.Brushes.Blue);
            AddColorItem("Yellow", System.Windows.Media.Brushes.Yellow);
            AddColorItem("Cyan", System.Windows.Media.Brushes.Cyan);
            AddColorItem("Magenta", System.Windows.Media.Brushes.Magenta);

            colorMenu.DropDownItems.Add("-");

            var customItem = new ToolStripMenuItem("Custom...");
            customItem.Click += (s, args) =>
            {
                using (var colorDialog = new ColorDialog())
                {
                    colorDialog.AllowFullOpen = true;
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        var dColor = colorDialog.Color;
                        var wpfColor = System.Windows.Media.Color.FromArgb(dColor.A, dColor.R, dColor.G, dColor.B);
                        var brush = new System.Windows.Media.SolidColorBrush(wpfColor);
                        BorderColorChanged?.Invoke(this, brush);
                    }
                }
            };
            colorMenu.DropDownItems.Add(customItem);

            _contextMenu.Items.Add(colorMenu);

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
