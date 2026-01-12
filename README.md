# QuickFieldToggle

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![LaunchBox](https://img.shields.io/badge/LaunchBox-Plugin-orange.svg)](https://www.launchbox-app.com/)
[![Version](https://img.shields.io/badge/Version-1.0.0-green.svg)](https://github.com/brandontravis/launchbox-quick-field-toggle/releases)

A powerful LaunchBox plugin for rapidly managing custom fields via the right-click context menu.

![QuickFieldToggle Demo](assets/screenshot.png)

## ✨ Features

- **⚡ Single-click field toggling** - No more Edit → Custom Fields → scroll → save
- **✓ Visual status indicators** - Checkmarks show current field state
- **🔗 Multi-field actions** - One click can set/remove multiple fields
- **📋 Multi-value picker** - Select from semicolon-separated value lists
- **🎯 Conditional display** - Show/hide items based on platform, genre, etc.
- **🎨 Custom icons** - Platform icons, playlist icons, or custom images
- **🔄 Hot reload** - Update config without restarting LaunchBox
- **📝 JSON configuration** - No recompilation needed

## 📦 Quick Install

1. Download the [latest release](https://github.com/brandontravis/launchbox-quick-field-toggle/releases/latest)
2. Extract to `LaunchBox\Plugins\`:
   ```
   Plugins\
   ├── QuickFieldToggle.dll
   └── quickfieldtoggle.json  (rename from .sample.json)
   ```
3. Restart LaunchBox
4. Right-click any game → See your new menu options!

> **Windows Security:** If the plugin doesn't load, right-click the DLL → Properties → Unblock

## 🚀 Quick Start

Rename `quickfieldtoggle.sample.json` to `quickfieldtoggle.json` for a working demo with **On Deck** and **Backlog** fields.

## 📖 Documentation

| Guide | Description |
|-------|-------------|
| [**About**](docs/ABOUT.md) | Why this plugin exists and what problems it solves |
| [**Configuration Guide**](docs/CONFIGURATION.md) | Complete setup guide with examples |
| [**Quick Reference**](dist/README.md) | Condensed reference (included in download) |

**Jump to:**
- [Simple Toggles](docs/CONFIGURATION.md#simple-toggles)
- [Action Menus](docs/CONFIGURATION.md#action-menus-move-to-backlog)
- [Multi-Value Picker](docs/CONFIGURATION.md#multi-value-picker)
- [Conditional Display](docs/CONFIGURATION.md#conditional-display)
- [Icons](docs/CONFIGURATION.md#icons)

## 💡 Example Configuration

```json
{
  "groups": [
    {
      "groupName": "Play Queue",
      "icon": "default",
      "iconCascade": "inherit",
      "items": [
        {
          "fieldName": "Now Playing",
          "menuLabel": "Move to Now Playing",
          "operationType": "set",
          "additionalActions": [
            { "field": "Backlog", "action": "remove" }
          ]
        }
      ]
    }
  ]
}
```

## 🛠️ Building from Source

```bash
# Requires Visual Studio 2022+ with .NET Framework 4.8
cd src
dotnet build -c Release
```

You'll need `Unbroken.LaunchBox.Plugins.dll` from your LaunchBox installation.

## 📁 Repository Structure

```
QuickFieldToggle/
├── LICENSE
├── README.md              ← You are here
├── assets/                ← Screenshots
│   └── screenshot.png
├── docs/                  ← Documentation
│   ├── ABOUT.md           ← Background & philosophy
│   └── CONFIGURATION.md   ← Complete config guide
├── dist/                  ← Release files (download these)
│   ├── QuickFieldToggle.dll
│   ├── quickfieldtoggle.sample.json
│   ├── quickfieldtoggle.sample.robust.json
│   └── README.md          ← Quick reference
└── src/                   ← Source code
    ├── QuickFieldToggle.csproj
    └── QuickFieldTogglePlugin.cs
```

## 🤝 Contributing

Contributions welcome! Feel free to:
- Report bugs
- Suggest features
- Submit pull requests

## 📄 License

[MIT License](LICENSE) - Free to use, modify, and distribute.

## 🙏 Credits

Developed for the LaunchBox community.

---

**[⬇️ Download Latest Release](https://github.com/brandontravis/launchbox-quick-field-toggle/releases/latest)**
