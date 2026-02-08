<div align="center">
  <img src="PinMe/Assets/icon.png" alt="Pinnie Icon" width="128" height="128">

  # Pinnie
  **The Cute & Powerful "Always On Top" Utility for Windows**

  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
  [![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)]()
  [![Status](https://img.shields.io/badge/status-active-success.svg)]()
</div>

---

## What is Pinnie?

**Pinnie** is more than just a tool to keep windows on top. It adds a touch of personality to your workflow! Pin any window with a simple hotkey, and watch as a cute animated pet (like a Capybara!) perches on top of it.

Whether you're a developer needing reference docs visible, or just want a cute companion while you work, Pinnie makes window management fun and effortless.

## ✨ Features

*   **📌 Instant Pinning**: Press **`Ctrl` + `Alt` + `T`** to toggle "Always On Top" for any active window.
*   **🐾 Pet Overlays**: Choose from built-in animated companions:
    *   **Capybara** (Default - Chill vibes)
    *   **Cat** (Curious and cute)
    *   **Guinea Pig** (Tiny and fast)
    *   *...or import your own GIF!*
*   **🎨 Custom Borders**:
    *   Add a visual border to pinned windows so you never lose track.
    *   Customize **Color**, **Thickness**, and **Corner Radius**.
*   **🔊 Sound Effects**: Satisfying audio cues when pinning and unpinning items.
*   **⚙️ System Tray Control**:
    *   Quickly change pets, adjust sizes, or toggle settings from the tray.
    *   **Startup Handling**: Run Pinnie automatically when Windows starts.
*   **🚀 Portable & Robust**:
    *   **Single Executable**: All assets are embedded. No messy folders required.
    *   **Smart Startup**: Prevents duplicate instances and notifies you if it's already running.
    *   **High-Res Support**: Works perfectly on 2K/4K monitors without glitchy borders.

## 🛠️ Installation & Usage

### 1. Download
Download the latest `Pinnie.exe` release. It's a single, portable file.

### 2. Run
Double-click `Pinnie.exe`. You'll encounter a friendly startup notification!
> "Pinnie Added Successfully!"

### 3. Usage
*   **Pin a Window**: Click any window to focus it, then press **`Ctrl` + `Alt` + `T`**.
    *   *Result*: The window stays on top, a border appears, and your chosen pet sits on it.
*   **Unpin**: Press the hotkey again.
*   **Tray Menu**: Right-click the Pinnie icon in your system tray to:
    *   Change the Pet Icon.
    *   Adjust Border Settings.
    *   Toggle Sound or Startup options.

## ⌨️ Default Hotkey

| Action | Hotkey |
| :--- | :--- |
| **Toggle Pin/Unpin** | `Ctrl` + `Alt` + `T` |

*(This can be customized in the settings if needed)*

## 🏗️ Building from Source

Requirements: **.NET 8 SDK**

```powershell
# Clone the repository
git clone https://github.com/Sunny-Hasho/PinMe.git

# Navigate to project
cd PinMe/PinMe

# Publish as a single file
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

## 📄 License
Pinnie is open-source software licensed under the [MIT License](LICENSE).

---
<div align="center">
  <sub>Made  by Sunny Hasho</sub>
</div>
