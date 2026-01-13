# Configuration Reference

This is the complete reference for `quickfieldtoggle.json`. For real-world examples and context, see [My Library Walkthrough](walkthrough.md).

---

## File Location

Place your config file at:
```
LaunchBox\Plugins\QuickFieldToggle\quickfieldtoggle.json
```

After making changes, reload via **Tools → Reload Quick Field Toggle Config** (no restart needed).

---

## Basic Structure

```json
{
  "defaultIcon": "default",
  "defaultIconCascade": "inherit",
  "groups": [...],
  "ungroupedItems": [...]
}
```

| Property | Description |
|----------|-------------|
| `defaultIcon` | Default icon for groups (see [Icons](#icons)) |
| `defaultIconCascade` | Default cascade behavior: `"none"` or `"inherit"` |
| `groups` | Array of menu groups (submenus) |
| `ungroupedItems` | Array of items that appear directly in the context menu |

---

## Groups

Groups create submenus in the context menu.

```json
{
  "groupName": "Play Queue",
  "icon": "default",
  "iconCascade": "inherit",
  "enabled": true,
  "conditions": [...],
  "items": [...]
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `groupName` | Yes | Submenu label |
| `enabled` | No | `true` (default) or `false` to disable |
| `icon` | No | Icon for this group (inherits from `defaultIcon`) |
| `iconCascade` | No | `"inherit"` (items get group icon) or `"none"` |
| `conditions` | No | Show group only when conditions match (see [Conditions](#conditions)) |
| `items` | Yes | Array of menu items |

---

## Items

Items are the actual menu entries that do things.

### Basic Toggle

```json
{
  "fieldName": "Discovery Bin",
  "menuLabel": "Discovery Bin",
  "operationType": "toggle",
  "enabled": true
}
```

| Property | Required | Description |
|----------|----------|-------------|
| `fieldName` | Yes | Custom field name (created automatically if it doesn't exist) |
| `menuLabel` | Yes | Text shown in the menu |
| `operationType` | No | `"toggle"` (default), `"set"`, or `"remove"` |
| `enabled` | No | `true` (default) or `false` |
| `toggledMenuLabel` | No | Alternative label when field is active (e.g., `"✓ Now Playing"`) |
| `icon` | No | Override icon for this item |
| `conditions` | No | Show item only when conditions match |
| `additionalActions` | No | Other fields to set/remove (see below) |

### Operation Types

| Type | Behavior |
|------|----------|
| `toggle` | Adds field if missing, removes if present |
| `set` | Always sets the field (never removes) |
| `remove` | Always removes the field |

Use `set` for "Move to..." actions where you want the action to always apply, not toggle off.

### Additional Actions

Modify multiple fields with one click:

```json
{
  "fieldName": "Now Playing",
  "menuLabel": "Move to Now Playing",
  "operationType": "set",
  "additionalActions": [
    { "field": "Backlog", "action": "remove" },
    { "field": "On Deck", "action": "remove" }
  ]
}
```

| Property | Description |
|----------|-------------|
| `field` | Field name to modify |
| `action` | `"set"` or `"remove"` |
| `value` | Value to set (default: `"true"`) |

---

## Multi-Value Mode

For fields with semicolon-separated values (like `"GOTY; Best RPG; Critics Choice"`), use multi-value mode to get a checkable submenu.

```json
{
  "fieldName": "Awards Won",
  "menuLabel": "Set Awards Won",
  "mode": "multiValue",
  "valueSource": "field",
  "enabled": true
}
```

| Property | Description |
|----------|-------------|
| `mode` | Set to `"multiValue"` |
| `valueSource` | `"field"` (scan library) or `"config"` (use `values` array) |
| `values` | Array of values when using `valueSource: "config"` |

### Value Source: Field

Scans your entire library for existing values in that field. The submenu shows all unique values found.

### Value Source: Config

Uses a predefined list:

```json
{
  "fieldName": "Generation",
  "menuLabel": "Set Generation",
  "mode": "multiValue",
  "valueSource": "config",
  "values": ["1", "2", "3", "4", "5", "6", "7", "8", "9"]
}
```

---

## Conditions

Show groups or items only when certain conditions are met.

```json
{
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

### Operators

| Operator | Description |
|----------|-------------|
| `equals` | Exact match |
| `notEquals` | Not exact match |
| `contains` | Substring match |
| `notContains` | No substring match |
| `exists` | Field has any value |
| `notExists` | Field is empty |
| `in` | Value in comma-separated list |
| `notIn` | Value not in list |

### Logic

Within a condition group:
- `"logic": "or"` — any rule can pass
- `"logic": "and"` — all rules must pass

Between condition groups: **AND** (all groups must pass)

### Complex Example

Show only for (Nintendo OR PlayStation) AND (has a Developer):

```json
{
  "conditions": [
    {
      "logic": "or",
      "rules": [
        { "field": "Platform", "operator": "contains", "value": "Nintendo" },
        { "field": "Platform", "operator": "contains", "value": "PlayStation" }
      ]
    },
    {
      "logic": "or",
      "rules": [
        { "field": "Developer", "operator": "exists" }
      ]
    }
  ]
}
```

### Supported Fields

Conditions work with:
- **Built-in fields:** Platform, Developer, Publisher, Genre, Series, Region, Title
- **Custom fields:** Any text-based custom field

**Not supported:** Date comparisons, numeric comparisons, boolean fields (Favorite, Hidden).

---

## Icons

```json
{
  "icon": "default"
}
```

| Format | Example | Description |
|--------|---------|-------------|
| `"default"` | — | Built-in QFT logo |
| `"media:Name"` | `"media:Nintendo Switch"` | Platform icon from your icon pack |
| `"path:file.png"` | `"path:icons/award.png"` | Custom file in plugin folder |

### Icon Cascade

- `"iconCascade": "inherit"` — Child items inherit the group's icon
- `"iconCascade": "none"` — Child items have no icon (unless explicitly set)

---

## Multi-Select Behavior

When multiple games are selected:

| Scenario | Result |
|----------|--------|
| **Toggle, all have field** | Removes from ALL |
| **Toggle, mixed or none** | Sets on ALL |
| **Set** | Sets on ALL |
| **Remove** | Removes from ALL |

The ✓ checkmark only appears when ALL selected games have the value.

---

## Hot Reload

After editing your config:

**Tools → Reload Quick Field Toggle Config**

No restart required. Changes take effect immediately.

---

## Troubleshooting

**Plugin doesn't load:**
- Right-click `QuickFieldToggle.dll` → Properties → Unblock
- Ensure .NET Framework 4.8 is installed

**Menu doesn't appear:**
- Check that `enabled: true` on your groups and items
- Verify JSON syntax (use a JSON validator)
- Check conditions aren't filtering out the menu

**Conditions not working:**
- Conditions are case-insensitive for text matching
- Use exact field names (check spelling)
- Date/numeric comparisons are not supported

---

## Full Example

See `quickfieldtoggle.sample-complex.json` for a complete working configuration.
