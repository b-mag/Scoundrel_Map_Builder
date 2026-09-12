# Roadmap — Builder + Scoundrel

Aligned with the Carcosa vision: large-world first-person RPG (King’s Field / Daggerfall energy), Lovecraft × Carcosa atmosphere, Doom 64 lighting, weather, and performance on Android.

**Priority lock:** finish **Step 1** (engine Doom 64 parity) before **Step 2** (Builder extras).  
Engine-only / not-yet-in-Builder ideas: `../scoundrel/FUTURE_WORK.md`.  
When extending this editor, stay additive — do not break stock Doom Builder 64 content.

## Step 1 — Engine meets Doom 64 basics

Goal: a map saved from **this Builder** loads and is walkable in Scoundrel.

| Priority | Work | Owner |
|----------|------|-------|
| P0 | Binary parity with `Doom64MapSetIO` + flat PWAD | Engine ✅ started |
| P0 | Sector bounds + light indices | Engine ✅ started |
| P0 | Drop a tiny Builder-exported PWAD into `assets/` and load it | Engine |
| P1 | Spawn player from thing type 1; basic impassable walls | Engine |
| P1 | 2S upper/lower/mid wall strips | Engine |
| P1 | MVP doors (open/close + use activation) | Engine |
| P2 | Texture hash table ↔ game textures | Engine + Builder config |
| P2 | Keys / simple thing pickups from map | Engine |
| P3 | Macros VM (subset) | Engine |

## Step 2 — Expand Doom Builder 64

Goal: author Carcosa-specific atmosphere and RPG content in the editor.

Track every Builder change in [CHANGELOG.md](CHANGELOG.md).

### 2a — Sector / map atmosphere

- [x] Weather preset per sector (CARCOSA `weatherPreset` — Sector Edit dropdown)
- [x] Fog color / density (CARCOSA fog fields — Sector Edit)
- Enhanced sky selection (sky flat / skybox id) — engine uses `F_SKY*` + per-region PNG
- [x] Optional outdoor flag (CARCOSA `outdoors` checkbox; also inferred from `F_SKY*`)

### 2b — Scripting & RPG

- Richer BLAM / SCRIPTS (or a Carcosa script layer) for:
  - NPC dialogue trees
  - Quest stages / flags
  - Triggers (enter sector, use, timer)
- Thing types for NPCs, quest givers, lore props
- Hub / exit metadata for large world streaming

### 2c — Editor UX

- Config includes for Carcosa specials (9001+) and new weather enums
- Preview notes in Help / our docs (not upstream README)
- Optional: export checklist (“engine can load this map”)

## Non-goals (near term)

- Full Doom 64EX feature parity before walkable Builder maps
- Shipping Midway commercial assets
- Replacing Kotlin showcase until WAD pipeline is proven
