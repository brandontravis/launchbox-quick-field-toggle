# About QuickFieldToggle

## The Problem

If you use custom fields to organize your LaunchBox library—tracking backlogs, tagging award winners, marking "now playing" games—you know the pain:

**Right-click → Edit → Custom Fields tab → scroll → find field → change value → OK → repeat for next game...**

It's tedious. Especially when you're going through your library tagging games or managing a play queue. What should be a one-click action becomes a multi-step ordeal.

## The Solution

**QuickFieldToggle** puts your custom field actions directly in the right-click context menu. One click. Done.

![QuickFieldToggle in action](../assets/screenshot.png)

## Why I Built This

I maintain a large LaunchBox library with extensive custom field usage:

- **Play Queue** - Tracking what I'm playing, what's on deck, and my backlog
- **Awards** - Tagging games with GOTY wins, nominations, and other accolades
- **Discovery Features** - Flagging games for my "discovery bin" rotation
- **Platform-Specific Data** - First-party titles, Virtual Console releases, mini console appearances

Managing all of this through the Edit dialog was slow and frustrating. I wanted the same kind of quick-toggle experience you get with favorites (the star), but for ANY custom field.

QuickFieldToggle is the result.

## Design Philosophy

### Configuration Over Code

Everything is controlled via a JSON file. No recompilation needed. Want to add a new toggle? Edit the JSON. Want to reorganize your menus? Edit the JSON. 

### Smart Defaults, Full Control

The plugin tries to do the right thing automatically:
- Checkmarks show current state
- Multi-select works intelligently (set on all if mixed, remove from all if unanimous)
- Icons inherit from parent groups

But you can override any behavior when needed.

### Respect the User's Setup

- Uses your active Platform Icon pack
- Reads LaunchBox settings for consistency
- Hot reload means no restarts required

## Use Cases

### Play Queue Management

Create mutually exclusive states: "Move to Now Playing" sets the Now Playing field AND removes Backlog and On Deck. One click, three field changes.

### Award Tracking

Semicolon-separated field values become checkable submenus. See all awards in your library, check the ones that apply to this game.

### Platform-Specific Tools

Show "First Party" toggle only for Nintendo games. Show "PSN Classic" only for PlayStation. The menu adapts to what you're looking at.

### Bulk Tagging

Select 50 games, right-click, toggle a field. All 50 updated instantly.

## Getting Started

Ready to try it? Head to the [Configuration Guide](CONFIGURATION.md) for detailed setup instructions and examples.

---

[← Back to README](../README.md)

