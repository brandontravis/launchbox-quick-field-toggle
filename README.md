# QuickFieldToggle

A LaunchBox plugin for managing custom fields without the friction.

---

## Why This Exists

I have 31,000 games across 70+ platforms in LaunchBox. I use custom fields extensively—play queues, award tracking, era classifications, platform-specific tags. It's a system that works well, except for one thing:

**Actually maintaining it is miserable.**

LaunchBox's built-in UI for custom fields requires drilling into each game's edit screen, navigating to the Custom Fields tab, scrolling to find the right field, making the change, and saving. That's 6+ clicks per field, per game. At scale, the friction becomes unbearable.

QuickFieldToggle puts those actions in the right-click menu. One click, done.

---

## What It Does

- **Toggle custom fields instantly** from the context menu
- **Create fields on-the-fly** — no pre-setup required
- **Multi-field actions** — "Move to Now Playing" sets one field and clears others
- **Multi-value picker** — for semicolon-separated fields, check/uncheck values from a submenu
- **Conditional menus** — show Nintendo tools only for Nintendo games
- **Visual indicators** — checkmarks show current state
- **Custom icons** — platform icons, playlist icons, or your own
- **Hot reload** — edit your config without restarting LaunchBox

---

## Quick Start

1. Download the [latest release](https://github.com/brandontravis/launchbox-quick-field-toggle/releases)
2. Extract the `QuickFieldToggle` folder to `LaunchBox\Plugins\`
3. Rename `quickfieldtoggle.sample-simple.json` to `quickfieldtoggle.json`
4. Restart LaunchBox
5. Right-click any game

That's it. You now have a "Play Queue" submenu with On Deck and Backlog toggles.

---

## Sample Configurations

Two samples are included:

| File | Description |
|------|-------------|
| `sample-simple.json` | Basic play queue (On Deck / Backlog) |
| `sample-complex.json` | Full working config with awards, conditional menus, multi-value pickers |

The complex sample is my actual configuration for managing 31k games. It's not a sanitized demo—it's real usage.

---

## Documentation

| Doc | Description |
|-----|-------------|
| [Configuration Reference](docs/configuration.md) | All options, operators, and settings |
| [My Library Walkthrough](docs/walkthrough.md) | How I use QFT with 31k games—philosophy, fields, and real examples |

---

## A Taste of the Config

**Mutually exclusive play queue:**

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

**Conditional Nintendo-only menu:**

```json
{
  "groupName": "Nintendo Tools",
  "conditions": [
    { "logic": "or", "rules": [
      { "field": "Platform", "operator": "contains", "value": "Nintendo" }
    ]}
  ]
}
```

**Multi-value award picker:**

```json
{
  "fieldName": "Awards Won",
  "mode": "multiValue",
  "valueSource": "field"
}
```

See the [Configuration Reference](docs/configuration.md) for the complete syntax.

---

## Requirements

- LaunchBox 13.24+ (Windows)
- .NET Framework 4.8

---

## License

MIT — use it, modify it, share it.

---

## Feedback

Found a bug? Have an idea? [Open an issue](https://github.com/brandontravis/launchbox-quick-field-toggle/issues) or find me in the LaunchBox forums.
