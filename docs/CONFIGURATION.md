# Configuration Guide

This guide covers everything you need to know to configure QuickFieldToggle for your workflow.

## Table of Contents

- [Basic Setup](#basic-setup)
- [Simple Toggles](#simple-toggles)
- [Action Menus](#action-menus-move-to-backlog)
- [Multi-Value Picker](#multi-value-picker)
- [Conditional Display](#conditional-display)
- [Icons](#icons)
- [Hot Reload](#hot-reload)
- [Multi-Select Behavior](#multi-select-behavior)
- [Complete Reference](#complete-reference)

---

## Basic Setup

The plugin uses a JSON configuration file (`quickfieldtoggle.json`) in your LaunchBox `Plugins` folder.

### File Structure

```json
{
  "groups": [
    {
      "groupName": "My Menu Group",
      "icon": "default",
      "iconCascade": "inherit",
      "enabled": true,
      "items": [...]
    }
  ],
  "ungroupedItems": [...]
}
```

| Section | Description |
|---------|-------------|
| `groups` | Creates submenus in the context menu |
| `ungroupedItems` | Items that appear directly in the context menu (no submenu) |

### Quick Start

1. Copy `quickfieldtoggle.sample.json` to `quickfieldtoggle.json`
2. Edit to match your custom fields
3. Restart LaunchBox (or use Hot Reload)

---

## Simple Toggles

The most basic use case—toggle a true/false custom field on and off.

### Example

```json
{
  "groups": [
    {
      "groupName": "Quick Tags",
      "icon": "default",
      "iconCascade": "inherit",
      "enabled": true,
      "items": [
        {
          "fieldName": "Discovery Bin",
          "menuLabel": "Discovery Bin",
          "enableValue": "true",
          "operationType": "toggle",
          "enabled": true
        },
        {
          "fieldName": "The Best",
          "menuLabel": "The Best",
          "enableValue": "true",
          "operationType": "toggle",
          "enabled": true
        }
      ]
    }
  ]
}
```

### How It Works

- **First click** → Sets "Discovery Bin" custom field to "true"
- **Second click** → Removes the field entirely
- **Visual indicator** → A ✓ checkmark appears when the field is set

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `fieldName` | string | *required* | The custom field name in LaunchBox |
| `menuLabel` | string | *required* | Text shown in the menu |
| `enableValue` | string | `"true"` | Value to set when enabling |
| `operationType` | string | `"toggle"` | See [Operation Types](#operation-types) |
| `enabled` | boolean | `true` | Show/hide this item |

---

## Action Menus (Move to Backlog)

For mutually exclusive states like a play queue, use `operationType: "set"` with `additionalActions` to create one-click actions that modify multiple fields.

### Example

```json
{
  "groupName": "Play Queue",
  "icon": "default",
  "iconCascade": "inherit",
  "enabled": true,
  "items": [
    {
      "fieldName": "Now Playing",
      "menuLabel": "Move to Now Playing",
      "toggledMenuLabel": "✓ Now Playing",
      "enableValue": "true",
      "operationType": "set",
      "enabled": true,
      "additionalActions": [
        { "field": "Backlog", "action": "remove" },
        { "field": "On Deck", "action": "remove" }
      ]
    },
    {
      "fieldName": "On Deck",
      "menuLabel": "Move to On Deck",
      "toggledMenuLabel": "✓ On Deck",
      "enableValue": "true",
      "operationType": "set",
      "enabled": true,
      "additionalActions": [
        { "field": "Now Playing", "action": "remove" },
        { "field": "Backlog", "action": "remove" }
      ]
    },
    {
      "fieldName": "Backlog",
      "menuLabel": "Move to Backlog",
      "toggledMenuLabel": "✓ On Backlog",
      "enableValue": "true",
      "operationType": "set",
      "enabled": true,
      "additionalActions": [
        { "field": "Now Playing", "action": "remove" },
        { "field": "On Deck", "action": "remove" }
      ]
    }
  ]
}
```

### Key Concepts

| Property | Purpose |
|----------|---------|
| `operationType: "set"` | Always sets the value (never toggles off) |
| `additionalActions` | Removes other queue states automatically |
| `toggledMenuLabel` | Shows "✓ On Backlog" instead of "✓ Move to Backlog" when active |

### Additional Actions

```json
"additionalActions": [
  { "field": "SomeField", "action": "remove" },
  { "field": "AnotherField", "action": "set", "value": "Active" }
]
```

| Action | Description |
|--------|-------------|
| `"remove"` | Removes the field entirely |
| `"set"` | Sets the field to specified value (default: `"true"`) |

> **Note:** Additional actions only fire when the primary field is being SET, not when it's being removed during a toggle operation.

---

## Multi-Value Picker

For fields containing semicolon-separated values (e.g., `"GOTY 2023; Best RPG; Critics Choice"`), use multi-value mode to show a submenu with checkable options.

### Example: Read Values from Library

```json
{
  "fieldName": "Awards Won",
  "menuLabel": "Set Awards Won",
  "mode": "multiValue",
  "valueSource": "field",
  "enabled": true,
  "additionalActions": [
    { "field": "Any Award Won", "action": "set", "value": "true" }
  ]
}
```

**How it works:**
1. Scans your **entire library** for existing values in "Awards Won"
2. Shows a submenu with all unique values found
3. Check/uncheck values to add/remove them from the current game
4. `additionalActions` fires when any value is added

### Example: Predefined Values

```json
{
  "fieldName": "Priority",
  "menuLabel": "Set Priority",
  "mode": "multiValue",
  "valueSource": "config",
  "values": ["High", "Medium", "Low", "None"],
  "enabled": true
}
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `mode` | string | `"simple"` (default) or `"multiValue"` |
| `valueSource` | string | `"field"` (scan library) or `"config"` (use values array) |
| `values` | array | Predefined values when using `valueSource: "config"` |

---

## Conditional Display

Show menu items only for specific platforms, genres, or any field value.

### Basic Example

```json
{
  "groupName": "Nintendo Tools",
  "icon": "media:Nintendo Switch",
  "iconCascade": "inherit",
  "enabled": true,
  "conditions": [
    {
      "logic": "or",
      "rules": [
        { "field": "Platform", "operator": "equals", "value": "Nintendo Wii" },
        { "field": "Platform", "operator": "equals", "value": "Nintendo Wii U" },
        { "field": "Platform", "operator": "equals", "value": "Nintendo 3DS" },
        { "field": "Platform", "operator": "equals", "value": "Nintendo Switch" }
      ]
    }
  ],
  "items": [
    {
      "fieldName": "First Party",
      "menuLabel": "First Party",
      "enableValue": "true",
      "operationType": "toggle",
      "enabled": true
    }
  ]
}
```

**This entire menu group only appears when right-clicking Nintendo games!**

### Condition Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `equals` | Exact match (case-insensitive) | `"value": "Nintendo Switch"` |
| `notEquals` | Not equal to | `"value": "Windows"` |
| `contains` | Contains text | `"value": "Nintendo"` |
| `notContains` | Does not contain | `"value": "Action"` |
| `exists` | Field has any value | (no value needed) |
| `notExists` | Field is empty or missing | (no value needed) |
| `in` | Value in comma-separated list | `"value": "Wii,Switch,3DS"` |
| `notIn` | Value not in list | `"value": "Windows,MS-DOS"` |

### Available Fields

Conditions can check any `IGame` property:

- `Platform`, `Title`, `Developer`, `Publisher`, `Series`, `Region`
- `GenresString`, `PlayMode`, `ReleaseYear`
- `Favorite`, `Hide`, `Installed`, `StarRating`, `PlayCount`
- **Any custom field name** (automatically falls back to custom field lookup)

### Complex Conditions

**Multiple rules with OR logic** (any rule can match):

```json
"conditions": [
  {
    "logic": "or",
    "rules": [
      { "field": "Platform", "operator": "contains", "value": "Nintendo" },
      { "field": "Platform", "operator": "contains", "value": "Sega" }
    ]
  }
]
```

**Multiple condition groups with AND logic** (all groups must match):

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

This shows the menu only for **Nintendo RPGs** (must match BOTH groups).

### Item-Level Conditions

Individual items within a group can have their own conditions:

```json
{
  "groupName": "Platform Tools",
  "enabled": true,
  "items": [
    {
      "fieldName": "PSN Classic",
      "menuLabel": "PSN Classic",
      "conditions": [
        {
          "rules": [
            { "field": "Platform", "operator": "contains", "value": "PlayStation" }
          ]
        }
      ]
    },
    {
      "fieldName": "Xbox BC",
      "menuLabel": "Xbox Backward Compatible",
      "conditions": [
        {
          "rules": [
            { "field": "Platform", "operator": "contains", "value": "Xbox" }
          ]
        }
      ]
    }
  ]
}
```

---

## Icons

Add visual polish with icons on groups and menu items.

### Icon Formats

| Format | Example | Description |
|--------|---------|-------------|
| `"default"` | `"icon": "default"` | Built-in QuickFieldToggle logo (16x16) |
| `"media:Name"` | `"icon": "media:Nintendo Switch"` | Icon from your Platform Icons media pack |
| `"platform:Name"` | `"icon": "platform:Nintendo Switch"` | Alias for `media:` |
| `"playlist:Name"` | `"icon": "playlist:Favorites"` | Playlist's icon from LaunchBox |
| `"path:file.png"` | `"icon": "path:icons/custom.png"` | Custom file relative to Plugins folder |

### Icon Cascade (Inheritance)

Set icons at the group level and have child items inherit:

```json
{
  "groupName": "Play Queue",
  "icon": "default",
  "iconCascade": "inherit",
  "items": [
    { "menuLabel": "Item 1" },
    { "menuLabel": "Item 2" },
    { "menuLabel": "Custom", "icon": "media:Star" }
  ]
}
```

| Value | Behavior |
|-------|----------|
| `"inherit"` | Items without icons inherit the group's icon |
| `"none"` | Items without icons have no icon (default) |

Items can always override with their own `icon` property.

### Platform Icon Search

The plugin intelligently searches your Platform Icon packs:

1. **Your active pack first** (from LaunchBox settings)
2. **Subdirectories included** (e.g., `Playlists/`, `Categories/`)
3. **Fallback to other packs** if not found

Custom icons like `savvy.png` in your icon pack's `Playlists` folder can be referenced simply as `"media:savvy"`.

### Custom Icons Best Practice

For custom icons you create, use `path:` and store them in your Plugins folder:

```
Plugins/
├── QuickFieldToggle.dll
├── quickfieldtoggle.json
└── icons/
    ├── custom1.png
    └── custom2.png
```

This keeps your icons safe from being overwritten when media packs are updated.

**Specifications:**
- **Size:** 16x16 pixels recommended (larger images are resized)
- **Format:** PNG with transparency, ICO, or BMP

---

## Hot Reload

Update your configuration without restarting LaunchBox:

**Tools → Reload Quick Field Toggle Config**

This reloads `quickfieldtoggle.json` and clears the icon cache. Perfect for testing configuration changes.

---

## Multi-Select Behavior

When multiple games are selected:

| Operation | Behavior |
|-----------|----------|
| Toggle (all have field) | Remove from ALL |
| Toggle (mixed or none have field) | Set on ALL |
| Set | Set on ALL |
| Remove | Remove from ALL |
| Multi-value (all have value) | Remove value from ALL |
| Multi-value (mixed or none) | Add value to ALL |

Visual indicators (checkmarks) show `✓` only when **ALL** selected games have the field/value.

---

## Complete Reference

### Operation Types

| Type | Behavior | Use Case |
|------|----------|----------|
| `toggle` | If all selected have value, remove. Otherwise, set. | Standard on/off fields |
| `set` | Always set the value (never removes) | Mutually exclusive states, actions |
| `remove` | Always remove the field | Cleanup actions |

### Group Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `groupName` | string | *required* | Submenu label |
| `enabled` | boolean | `true` | Show/hide entire group |
| `icon` | string | null | Icon specification |
| `iconCascade` | string | `"none"` | `"inherit"` or `"none"` |
| `conditions` | array | `[]` | Conditional display rules |
| `items` | array | *required* | Menu items in this group |

### Item Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `fieldName` | string | *required* | Custom field name |
| `menuLabel` | string | *required* | Menu text |
| `toggledMenuLabel` | string | null | Alternative label when enabled |
| `enableValue` | string | `"true"` | Value to set |
| `operationType` | string | `"toggle"` | `toggle`, `set`, or `remove` |
| `mode` | string | `"simple"` | `simple` or `multiValue` |
| `valueSource` | string | `"config"` | `config` or `field` |
| `values` | array | `[]` | Values for multiValue mode |
| `enabled` | boolean | `true` | Show/hide item |
| `icon` | string | null | Icon specification |
| `additionalActions` | array | `[]` | Additional field changes |
| `conditions` | array | `[]` | Conditional display rules |

### Sample Files

The download includes two sample configurations:

| File | Description |
|------|-------------|
| `quickfieldtoggle.sample.json` | Simple demo with On Deck / Backlog |
| `quickfieldtoggle.sample.robust.json` | Every feature demonstrated with comments |

---

## Need Help?

- **GitHub Issues:** [Report bugs or request features](https://github.com/brandontravis/launchbox-quick-field-toggle/issues)
- **LaunchBox Forums:** Post in the plugin thread

---

[← Back to README](../README.md) | [About QuickFieldToggle](ABOUT.md)

