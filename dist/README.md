# QuickFieldToggle

A LaunchBox plugin for rapidly managing custom fields via the right-click context menu. Toggle fields, execute multi-field actions, and organize your game library without navigating through edit dialogs.

## Features

- **Single-click field toggling** - No more Edit → Custom Fields → scroll → click → save
- **Visual status indicators** - Checkmarks show current field state
- **Multi-field actions** - One action can set/remove multiple fields
- **Multi-value picker** - Select from semicolon-separated values
- **Conditional display** - Show/hide menu items based on platform, genre, or any field
- **Custom icons** - Use LaunchBox platform/playlist icons or custom images
- **Hot reload** - Update config without restarting LaunchBox
- **JSON configuration** - No recompilation needed to customize

---

## Installation

1. Copy `QuickFieldToggle.dll` to your LaunchBox `Plugins` folder
2. Copy `quickfieldtoggle.json` to the same `Plugins` folder
3. Restart LaunchBox

**Windows Security Note:** If the plugin doesn't load, right-click the DLL → Properties → check "Unblock" → Apply.

---

## Quick Start

Rename `quickfieldtoggle.sample.json` to `quickfieldtoggle.json` for a working demo with "On Deck" and "Backlog" fields.

---

## Configuration Structure

```json
{
  "groups": [...],
  "ungroupedItems": [...]
}
```

### Groups

Groups appear as submenus in the right-click context menu:

```json
{
  "groupName": "Play Queue",
  "enabled": true,
  "conditions": [],
  "items": [...]
}
```

| Property | Type | Description |
|----------|------|-------------|
| `groupName` | string | Submenu label |
| `enabled` | boolean | Show/hide entire group |
| `conditions` | array | Optional conditions for when to show group |
| `items` | array | Menu items within this group |

### Ungrouped Items

Items in `ungroupedItems` appear directly in the context menu without a submenu wrapper.

---

## Menu Item Configuration

```json
{
  "fieldName": "On Deck",
  "menuLabel": "Move to On Deck",
  "toggledMenuLabel": "✓ On Deck",
  "enableValue": "true",
  "operationType": "set",
  "mode": "simple",
  "enabled": true,
  "additionalActions": [],
  "conditions": []
}
```

### Basic Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `fieldName` | string | *required* | Custom field name to modify |
| `menuLabel` | string | *required* | Text shown in menu |
| `toggledMenuLabel` | string | null | Alternative label when field is enabled (replaces checkmark) |
| `enableValue` | string | `"true"` | Value to set when enabling |
| `enabled` | boolean | `true` | Show/hide this item |

### Operation Types

| `operationType` | Behavior |
|-----------------|----------|
| `"toggle"` | **(Default)** If all selected games have the field, remove it. Otherwise, set it. |
| `"set"` | Always set the field value (one-way action, never removes) |
| `"remove"` | Always remove the field |

**When to use each:**

- **Toggle** - Standard on/off behavior: "Toggle Favorite", "Toggle Hidden"
- **Set** - Mutually exclusive states: "Move to Backlog" (sets Backlog, shouldn't toggle off)
- **Remove** - Cleanup actions: "Clear all tags"

### Visual Indicators

**Default behavior:** Shows `✓ Menu Label` when field is enabled for all selected games.

**Custom label:** Use `toggledMenuLabel` to show a different label instead:

```json
{
  "menuLabel": "Move to Backlog",
  "toggledMenuLabel": "✓ On Backlog"
}
```

**No indicator:** For `operationType: "set"`, no checkmark is shown (it's a one-way action).

---

## Multi-Field Actions

Execute additional field changes when the primary action fires:

```json
{
  "fieldName": "On Deck",
  "menuLabel": "Move to On Deck",
  "operationType": "set",
  "additionalActions": [
    { "field": "Backlog", "action": "remove" },
    { "field": "Now Playing", "action": "remove" },
    { "field": "Queue Status", "action": "set", "value": "Active" }
  ]
}
```

| Action | Description |
|--------|-------------|
| `"set"` | Set field to the specified `value` (default: `"true"`) |
| `"remove"` | Remove the field entirely |

**Note:** Additional actions only fire when the primary field is being SET, not when it's being removed (for toggle operations).

---

## Multi-Value Mode

For fields containing semicolon-separated values (e.g., `"Award1; Award2; Award3"`), use multi-value mode to show a submenu with checkable options:

```json
{
  "fieldName": "Awards",
  "menuLabel": "Edit Awards",
  "mode": "multiValue",
  "valueSource": "field",
  "additionalActions": [
    { "field": "Has Awards", "action": "set", "value": "true" }
  ]
}
```

### Value Sources

| `valueSource` | Description |
|---------------|-------------|
| `"field"` | Reads existing values from the field across all selected games |
| `"config"` | Uses predefined values from the `values` array |

**Config-based values:**

```json
{
  "fieldName": "Priority",
  "menuLabel": "Set Priority",
  "mode": "multiValue",
  "valueSource": "config",
  "values": ["High", "Medium", "Low", "None"]
}
```

---

## Conditional Display

Show/hide groups or items based on game properties.

### Basic Condition

```json
{
  "groupName": "Nintendo Tools",
  "conditions": [
    {
      "logic": "or",
      "rules": [
        { "field": "Platform", "operator": "contains", "value": "Nintendo" }
      ]
    }
  ]
}
```

### Condition Operators

| Operator | Description |
|----------|-------------|
| `equals` | Exact match (case-insensitive) |
| `notEquals` | Not an exact match |
| `contains` | Value contains the string |
| `notContains` | Value does not contain the string |
| `exists` | Field has any non-empty value |
| `notExists` | Field is empty or missing |
| `in` | Value is in comma-separated list: `"value": "A,B,C"` |
| `notIn` | Value is not in comma-separated list |

### Available Fields

Conditions can check any `IGame` property via reflection:

- `Platform`, `Title`, `Developer`, `Publisher`, `Series`, `Region`
- `GenresString`, `PlayMode`, `ReleaseYear`
- `Favorite`, `Hide`, `Installed`, `StarRating`
- Any custom field name (falls back to custom field lookup)

### Complex Conditions

**Multiple rules with OR logic:**

```json
{
  "logic": "or",
  "rules": [
    { "field": "Platform", "operator": "equals", "value": "Nintendo Wii" },
    { "field": "Platform", "operator": "equals", "value": "Nintendo Wii U" },
    { "field": "Platform", "operator": "equals", "value": "Nintendo Switch" }
  ]
}
```

**Multiple condition groups (AND between groups):**

```json
"conditions": [
  {
    "logic": "or",
    "rules": [
      { "field": "GenresString", "operator": "contains", "value": "RPG" }
    ]
  },
  {
    "logic": "or", 
    "rules": [
      { "field": "Platform", "operator": "contains", "value": "Nintendo" }
    ]
  }
]
```

This shows the menu only for Nintendo RPGs (must match BOTH groups).

### Item-Level Conditions

Individual items can have their own conditions:

```json
{
  "groupName": "Platform Tools",
  "items": [
    {
      "fieldName": "PSN Classic",
      "menuLabel": "PSN Classic",
      "conditions": [
        {
          "rules": [{ "field": "Platform", "operator": "contains", "value": "PlayStation" }]
        }
      ]
    }
  ]
}
```

---

## Icons

Add icons to groups and menu items using the `icon` property.

### Icon Cascade (Inheritance)

Use `iconCascade` on groups to have child items inherit the group's icon:

```json
{
  "groupName": "Play Queue",
  "icon": "default",
  "iconCascade": "inherit",
  "items": [
    { "menuLabel": "Item 1" },
    { "menuLabel": "Item 2" },
    { "menuLabel": "Override", "icon": "media:Custom" }
  ]
}
```

| Value | Behavior |
|-------|----------|
| `"inherit"` | Items without their own `icon` use the group's icon |
| `"none"` | Items without their own `icon` have no icon (default) |

**Note:** Items can always override with their own `icon` property.

### Icon Formats

| Format | Example | Description |
|--------|---------|-------------|
| `"default"` | `"icon": "default"` | Built-in QFT logo (16x16) |
| `"media:Name"` | `"icon": "media:Nintendo Switch"` | Icon from Platform Icons media pack |
| `"platform:Name"` | `"icon": "platform:Nintendo Switch"` | Alias for `media:` |
| `"playlist:Name"` | `"icon": "playlist:Favorites"` | Playlist's icon from LaunchBox data |
| `"path:file.png"` | `"icon": "path:icons/custom.png"` | **Custom icons (recommended)** - relative to Plugins folder |
| *(omit)* | | No icon |

**For custom icons, use `path:`** - Store them in a folder alongside the plugin DLL (e.g., `Plugins\icons\`). This keeps your custom icons safe from being overwritten when media packs are updated.

### Platform Icon Packs (Recommended)

Both `media:` and `platform:` search your **Platform Icon packs**:

```
LaunchBox\Images\Media Packs\Platform Icons\
├── Nostalgic Platform Icons\    ← Your active pack (checked FIRST)
├── Default\
├── [Your Other Packs]\
└── ...
```

**Smart Search Order:**
1. **Active pack first** - Reads your `PlatformIconPack` setting from `Data\Settings.xml`
2. **Subdirectories included** - Searches pack root AND subfolders (e.g., `Playlists\`, `Categories\`)
3. **Fallback to other packs** - Searches remaining packs if not found

This means:
- Icons from your active theme get priority (consistent with your LaunchBox look)
- Custom icons (like `savvy.png`) in subfolders are found automatically
- Just use `"media:savvy"` - no need to specify the `Playlists\` path
- Falls back to other packs if needed

```json
{
  "icon": "media:Nintendo Switch"
}
```

```json
{
  "icon": "media:savvy"
}
```

**To add custom icons:** Place them in your active Platform Icon pack folder (e.g., `savvy.png` for a custom playlist icon).

**Note:** Clear Logos are NOT used - only Platform Icons (correct 16x16ish size).

### Examples

**Group with platform icon:**

```json
{
  "groupName": "Nintendo Tools",
  "icon": "platform:Nintendo Switch",
  "items": [...]
}
```

**Item with default icon:**

```json
{
  "fieldName": "On Deck",
  "menuLabel": "Move to On Deck",
  "icon": "default"
}
```

**Custom icon file:**

```json
{
  "fieldName": "Favorite",
  "menuLabel": "Favorite",
  "icon": "path:icons/star.png"
}
```

### Icon Specifications

For custom icons:
- **Size:** 16x16 pixels recommended (will be resized if different)
- **Format:** PNG with transparency, ICO, or BMP
- **Location:** Relative to the Plugins folder, or absolute path

Icons are cached for performance. Use **Tools → Reload Quick Field Toggle Config** to refresh icons after changes.

---

## Hot Reload

Update your configuration without restarting LaunchBox:

**Tools → Reload Quick Field Toggle Config**

This reloads `quickfieldtoggle.json` and clears the icon cache. Useful for testing configuration changes.

---

## Multi-Select Behavior

When multiple games are selected:

| Operation | Behavior |
|-----------|----------|
| Toggle (all have field) | Remove from ALL |
| Toggle (mixed/none have field) | Set on ALL |
| Set | Set on ALL |
| Remove | Remove from ALL |
| Multi-value (all have value) | Remove value from ALL |
| Multi-value (mixed/none) | Add value to ALL |

Checkmarks show `✓` only when ALL selected games have the field/value.

---

## Sample Files

| File | Description |
|------|-------------|
| `quickfieldtoggle.sample.json` | Simple drop-in demo with On Deck / Backlog |
| `quickfieldtoggle.sample.robust.json` | Comprehensive example showing all features |

---

## Troubleshooting

### Plugin doesn't appear

1. Ensure DLL is in LaunchBox's `Plugins` folder
2. Check Windows didn't block the file (Properties → Unblock)
3. Restart LaunchBox completely

### Menu items not showing

1. Check `enabled: true` on items and groups
2. Verify condition rules match your game
3. Use Tools → Reload Config after changes

### Changes not saving

The plugin calls `PluginHelper.DataManager.Save(false)` after each action. If changes don't persist, check LaunchBox's data folder permissions.

### JSON syntax errors

Use a JSON validator. Common issues:
- Trailing commas after last item in arrays
- Missing quotes around strings
- Unescaped special characters

---

## Building from Source

Requires:
- Visual Studio 2022+
- .NET Framework 4.8 SDK
- Reference to `Unbroken.LaunchBox.Plugins.dll` from LaunchBox

```bash
cd QuickFieldToggle
dotnet build -c Release
```

Output: `bin/Release/QuickFieldToggle.dll`

---

## License

MIT License - Free to use, modify, and distribute.

---

## Credits

Developed with assistance from Claude (Anthropic) for LaunchBox community use.

