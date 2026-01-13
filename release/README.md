# QuickFieldToggle

A LaunchBox plugin for rapidly managing custom fields via the right-click context menu.

## Installation

1. Copy the entire `QuickFieldToggle` folder to `LaunchBox\Plugins\`
2. Rename `quickfieldtoggle.sample.json` to `quickfieldtoggle.json`
3. Restart LaunchBox
4. Right-click any game → See your new menu options!

**Windows Security:** If the plugin doesn't load, right-click `QuickFieldToggle.dll` → Properties → Unblock

## Folder Structure

```
Plugins\QuickFieldToggle\
├── QuickFieldToggle.dll
├── quickfieldtoggle.json       ← Your configuration (rename from .sample.json)
├── README.md
└── icons\                      ← Place custom icons here (optional)
```

## Configuration

Edit `quickfieldtoggle.json` to customize your menu items.

**Hot Reload:** After editing, use **Tools → Reload Quick Field Toggle Config** (no restart needed!)

## Features

- **Single-click field toggling** - No more Edit → Custom Fields → scroll → save
- **Visual status indicators** - Checkmarks show current field state
- **Multi-field actions** - One click can set/remove multiple fields
- **Multi-value picker** - Select from semicolon-separated values
- **Conditional display** - Show/hide based on platform, genre, etc.
- **Custom icons** - Platform icons, playlist icons, or your own

## Documentation

Full configuration guide: https://github.com/brandontravis/launchbox-quick-field-toggle

## License

MIT License - Free to use, modify, and distribute.

