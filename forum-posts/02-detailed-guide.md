# Forum Post 2: Detailed Configuration Guide

**Subject:** [GUIDE] QuickFieldToggle - Complete Configuration Guide with Examples

---

## Post Content:

This is the detailed configuration guide for **QuickFieldToggle**. If you haven't seen the announcement, check out the post above!

---

## Table of Contents

1. [Basic Setup](#basic-setup)
2. [Simple Toggle Example](#simple-toggle)
3. [Action Menus (Move to Backlog)](#action-menus)
4. [Multi-Value Picker (Awards)](#multi-value-picker)
5. [Conditional Display (Platform-Specific)](#conditional-display)
6. [Icons](#icons)
7. [Full Reference](#full-reference)

---

## Basic Setup

The plugin uses a JSON file (`quickfieldtoggle.json`) in your Plugins folder. Here's the basic structure:

```json
{
  "groups": [
    {
      "groupName": "My Menu Group",
      "items": [...]
    }
  ],
  "ungroupedItems": [...]
}
```

- **groups** - Creates submenus in the context menu
- **ungroupedItems** - Items that appear directly in the menu (no submenu)

**[📸 SCREENSHOT: Show a simple menu with one group containing 2-3 items]**

---

## Simple Toggle Example

The most basic use case - toggle a true/false custom field:

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

**How it works:**
- First click → Sets "Discovery Bin" to "true"
- Second click → Removes the field entirely
- A ✓ checkmark appears when the field is set

**[📸 SCREENSHOT: Show the Quick Tags menu with a checkmark next to one item]**

---

## Action Menus (Move to Backlog)

For mutually exclusive states (like a play queue), use `operationType: "set"` with `additionalActions`:

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

**Key points:**
- `operationType: "set"` - Always sets the value (never toggles off)
- `additionalActions` - Removes other queue states when you move a game
- `toggledMenuLabel` - Shows "✓ On Backlog" instead of "✓ Move to Backlog" when active

**[📸 SCREENSHOT: Show the Play Queue submenu with one item showing the toggled state]**

---

## Multi-Value Picker (Awards)

For fields with semicolon-separated values (like `"GOTY 2023; Best RPG; Critics Choice"`), use multi-value mode:

```json
{
  "groupName": "Set Awards",
  "icon": "default",
  "iconCascade": "inherit",
  "enabled": true,
  "items": [
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
  ]
}
```

**How it works:**
- Scans your ENTIRE library for existing values in "Awards Won"
- Shows a submenu with all unique values
- Check/uncheck values to add/remove them
- `additionalActions` can set a flag when any award is added

**[📸 SCREENSHOT: Show the Awards submenu with checkable values like "GOTY", "Best RPG", etc.]**

**Alternative: Predefined values**

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

---

## Conditional Display (Platform-Specific)

Show menu items only for specific platforms, genres, or any field value:

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

**This menu only appears when right-clicking Nintendo games!**

**[📸 SCREENSHOT: Show the Nintendo Tools menu appearing for a Switch game]**

**Available operators:**
| Operator | Description |
|----------|-------------|
| `equals` | Exact match |
| `notEquals` | Not equal |
| `contains` | Contains text |
| `notContains` | Doesn't contain |
| `exists` | Field has any value |
| `notExists` | Field is empty |
| `in` | Value in list: `"Wii,Switch,3DS"` |
| `notIn` | Value not in list |

**Available fields:** Platform, Title, Developer, Publisher, GenresString, Series, Region, ReleaseYear, Favorite, StarRating, or any custom field name!

---

## Icons

Add visual flair with icons:

```json
{
  "groupName": "Play Queue",
  "icon": "default",
  "iconCascade": "inherit"
}
```

**Icon formats:**
| Format | Example | Description |
|--------|---------|-------------|
| `"default"` | Built-in QFT logo | |
| `"media:Name"` | `"media:Nintendo Switch"` | Platform icon from your icon pack |
| `"playlist:Name"` | `"playlist:Favorites"` | Playlist icon |
| `"path:file.png"` | `"path:icons/custom.png"` | Custom file in Plugins folder |

**Icon Cascade:**
- `"iconCascade": "inherit"` - Child items inherit the group's icon
- `"iconCascade": "none"` - Child items have no icon (default)

**[📸 SCREENSHOT: Show menus with platform icons like Nintendo Switch]**

---

## Hot Reload

Made changes to your config? No need to restart LaunchBox!

**Tools → Reload Quick Field Toggle Config**

**[📸 SCREENSHOT: Show the Tools menu with the reload option]**

---

## Full Reference

### Operation Types

| Type | Behavior |
|------|----------|
| `toggle` | Toggle on/off (default) |
| `set` | Always set value (for actions like "Move to...") |
| `remove` | Always remove the field |

### Multi-Select Behavior

When multiple games are selected:
- **Toggle (all have field)** → Remove from ALL
- **Toggle (mixed/none)** → Set on ALL
- **Set** → Set on ALL
- Shows ✓ only when ALL selected games have the value

### Sample Files

The download includes:
- `quickfieldtoggle.sample.json` - Simple demo
- `quickfieldtoggle.sample.robust.json` - Every feature demonstrated

---

## Questions?

Reply here or open an issue on GitHub: https://github.com/brandontravis/launchbox-quick-field-toggle

Happy organizing! 🎮

