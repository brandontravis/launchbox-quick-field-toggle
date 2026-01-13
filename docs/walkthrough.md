# How I Actually Use This Thing

Full disclosure: I’m _that_ person who spends more time organizing games than actually playing them. If you’ve ever fallen down a tagging rabbit hole for days—or weeks—you already understand.

I’ve got roughly 31,000 games in LaunchBox spread across 70+ platforms. That’s… a lot. Decision paralysis is real. My early solution was to build playlists. Dozens of them. Eventually hundreds. Surely that would fix discoverability, right?

Turns out, not so much.

Playlists can be useful, but they demand a ton of intentionality up front. Worse, LaunchBox and BigBox don’t treat them like first-class citizens. In LaunchBox, if a playlist isn’t assigned a parent category, it basically disappears from normal browsing. In BigBox, half the themes assume every playlist will have beautiful, complete artwork. If you’re obsessive about visuals like I am, that is a massive barrier before you ever get to play anything.

But the real deal-breaker? Playlists are **silos**. 

You _can_ filter inside a playlist, but the playlist itself has no meaning outside its own walls. A manually curated “Backlog” playlist helps in that one space, but LaunchBox has no way to know what being “in the backlog” means anywhere else. If I later want to say: “Show me all 4.0+ Nintendo RPGs that are also in my backlog,” there’s no metadata hook. Auto-populated playlists run on predefined rules, and manual playlists are just lists — not tags, not status flags, not searchable attributes. They’re not entry points; they’re endpoints.

And that’s where filters and metadata come in.

LaunchBox’s search and filter system is incredibly powerful. Add custom fields to the mix, and suddenly you can slice your library any way you want on demand: “All Nintendo platformers, community rating 4.0+, under five hours on HLTB.” Boom. Instant discovery engine. No upfront curation required.

Unfortunately, the built-in metadata—and even the new progress tracking feature—didn’t cover how I personally think about backlog, rotation, or what I actually want to play next.

So I went all-in on custom fields. Lots of them. What I’m playing. What I want to play. Award winners. First-party vs third-party. What era. What wishlist pile it sits in.

You get the idea.

But here’s the catch: managing custom fields inside LaunchBox is painful. It’s six clicks to edit a single field for one game. Bulk editing takes even more clicks. When you’re trying to maintain consistency across thousands of titles, the friction wins every time. You simply stop keeping up.

QuickFieldToggle is what happens when an obsessive organizer gets tired of clicking and finally decides to learn C#.

---

## The Play Queue (aka "What Am I Even Playing?")

I can never remember what I'm supposed to be playing. So I made a system:

| Field | What It Means |
|-------|---------------|
| `Now Playing` | Games I'm actively playing (or telling myself I am) |
| `On Deck` | Next up. The "I'll definitely play this soon" pile |
| `Backlog` | The endless abyss of "someday" |

These fields are mutually exclusive—a game can't be in multiple queues. Before QFT, that meant manually removing one field before adding another. Now:

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

One click. Game moves to "Now Playing," automatically leaves the backlog. No more conflicting states.

---

## Awards

I track award-winning games. I don't know why. It started as "oh, this is neat data to have" and turned into a whole thing.

Two main fields:
- `Awards Won` — semicolon-separated list like "Game of the Year (TGA); Best RPG"
- `Award Nominations` — same format

Plus flags:
- `Any Award Won` — true if the game won anything
- `Any Award Nomination` — true if nominated

The multi-value picker scans my entire library for existing award values and shows them as a checkable list. When I add an award, it automatically sets the flag.

For GOTY awards specifically, I got tired of typing the same ceremony names, so:

```json
{
  "fieldName": "Awards Won",
  "menuLabel": "Set GOTY Awards",
  "mode": "multiValue",
  "valueSource": "config",
  "values": [
    "Game of the Year (D.I.C.E.)",
    "Game of the Year (GDC)",
    "Game of the Year (Golden Joystick)",
    "Game of the Year (TGA)",
    "Best Game (BAFTA)"
  ],
  "additionalActions": [
    { "field": "Any Award Won", "action": "set", "value": "true" }
  ]
}
```

Predefined list, one click each.

---

## Nintendo Stuff (Conditional Menus)

I have a "First Party" field for Nintendo games. In my head, tt only makes sense for Nintendo platforms, so why show it for PlayStation?

```json
{
  "groupName": "Nintendo Tools",
  "icon": "media:Nintendo Switch",
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
  ]
}
```

The whole menu group disappears when I'm looking at non-Nintendo games. Less clutter.

---

## Eras and Generations (I Told You I Was Obsessive)

I track:
- `Generation` — Console generation (1-9)
- `Platform Era` — "8-bit," "16-bit," "Modern Console," etc.
- `Platform Era Lifecycle` — Where in the era's lifespan a game released

Is this useful? Probably not. Do I enjoy having it organized? Absolutely.

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

## Mini Consoles and Virtual Console

I track which games appeared on mini consoles (NES Classic, PlayStation Classic, etc.) and Nintendo's Virtual Console. Why? Because it's interesting to see what Nintendo/Sony/Sega considered "the classics."

- `Mini Console Name` — Which mini consoles include this game
- `VC Platforms` — Which Virtual Console platforms had it

Both use multi-value pickers that scan the library for existing values.

---

## The Discovery Fields

Two simple toggles:
- `Discovery Bin` — Hidden gems I want to highlight
- `The Best` — Personal favorites

Nothing fancy. Just quick tags for standout games.

---

## The Sample File

The `sample-complex.json` is basically my real config. It's not a cleaned-up demo—it's what I actually use. You'll see my field names, my organizational quirks, all of it -- plus, some additional sample configs to show the full capability of CFT.

You don't have to copy it. Take what's useful, ignore what's not. The point is showing how these features work together in practice, not prescribing how you should organize your library.

If you're the type of person who reads documentation about game library organization... you probably already have opinions about how you want to set things up.

---

## Questions?

If you're stuck or want to bounce ideas about your setup, open an issue on GitHub or find me in the LaunchBox forums. Always happy to talk about this stuff with fellow organization nerds.
