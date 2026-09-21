# Vision — Carcosa on Scoundrel

## Product

A **first-person RPG engine on Android** (desktop for authoring/debug), used to build a large, atmospheric world inspired by:

- **King’s Field** — deliberate movement, dread, discovery
- **Elder Scrolls: Daggerfall** — scale, open exploration, quest structure
- **Doom 64** — colored sector lighting, oppressive architecture, macros-era interactivity

Setting: **Lovecraftian Carcosa** — yellow signs, mist, wrong geometry, beauty that unsettles.

## Pillars

1. **Atmosphere first** — lighting, fog, weather, sky, sound; mood must survive performance budgets.
2. **Large world** — sector maps + streaming; Builder is the long-term level tool.
3. **RPG bones** — inventory, spells, keys, NPCs, quests; deepen without losing FPS feel.
4. **Performance** — monitor FPS on device; prefer cheap screen-space weather and D64-style lights over heavy post.

## Tooling split

| Tool | Role |
|------|------|
| **Doom Builder 64 Enhanced** (this repo) | Map geometry, lights, things, macros → later weather/quests |
| **Scoundrel engine** (`scoundrel`) | Runtime: raycast/sector render, collision, RPG systems, Android |
| **Kotlin builders** | Prototype / showcase until WAD pipeline is solid |

## Success snapshots

- Walk a Builder-made chapel with correct walls, lights, and a working door.
- Stand outside in rain/mist with Doom 64–colored interiors visible through a doorway.
- Accept a quest from an NPC authored as a map thing + script, not hard-coded Kotlin.
