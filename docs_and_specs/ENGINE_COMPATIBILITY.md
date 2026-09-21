# Engine ↔ Doom Builder 64 compatibility

**Status (2026-09-10):** Geometry **parse path** is being brought in line with Builder.  
Gameplay simulation of stock D64 specials is still incomplete. **Runtime maps load from Builder PWADs** via `pack.json` + `Doom64SessionFactory`. Kotlin `CarcosaWorldBuilder` is a seed exporter only (`:game:exportCarcosaMaps`) and must not run as a live fallback.

## Ground truth

Builder PC map IO: `Source/Core/IO/Doom64MapSetIO.cs`  
Configs: `Build/Configurations/Doom64.cfg` + `Includes/D64_*.cfg`

## Lump set (flat PWAD)

| Lump | Builder | Scoundrel reader |
|------|---------|------------------|
| `MAP##` | Marker | Detected; siblings from `index+1` |
| `THINGS` | 14 bytes (x,y,z,angle,type,flags,tid) | ✓ (z + tid kept) |
| `LINEDEFS` | 16 bytes, **flags u32** | ✓ (fixed; was u16 — broke alignment) |
| `SIDEDEFS` | 12 bytes: x,y,**hi,lo,mid**,sector | ✓ (was single tex + pad) |
| `VERTEXES` | 8 bytes 16.16 | ✓ |
| `SECTORS` | 24 bytes | ✓ |
| `LIGHTS` | **6 bytes** RGBA + tag | ✓ (was 4-byte RGBA) |
| `MACROS` | Blindcopy / BLAM | Subset interpreter (`Doom64Macros`; ref EX+ `p_macros.c`) |
| `LEAFS` / nodes | Nodebuild | Ignored (raycast engine) |

## What works today

- Nested smoke-test PWAD + **flat** `MAP01` fixture (`buildMinimalFlatTestMap`)
- Procedural Carcosa showcase (thick walls, roofs, keyed doors, weather overlay, D64-style colored lights)
- Sector flags: water / damage (when present on loaded sectors)
- Carcosa specials 9001–9008 (rail, lava, fog volume, key door, warp wall, …)

## Critical gaps (maps “open and play”)

1. **Texture hashes** — expand catalog names as maps need; WAD PNG lumps preferred when present.
2. **Swing door styles** — Carcosa extras; stock D64 raise/split done.
3. **LEAFS / BSP** — optional; raycast may skip.

## Round-trip

```
Doom Builder 64 Enhanced  →  flat PWAD (+ optional T_/S_ PNG lumps)
        ↓
Doom64MapReader.read() + Doom64WadGraphics.extract()
        ↓
SectorGameWorld / SectorRenderer   [MACROS subset + specials MVP]
```

Kotlin `SectorMapBuilder` → Editor: **not supported** (needs exporter later).

## Verification checklist

- [x] Linedef / sidedef / lights layouts match `Doom64MapSetIO`
- [x] Flat `MAP##` loader
- [x] Light index `<256` / `≥256`
- [x] Sector AABB rebuild after load
- [x] Load walkable room PWAD in-game (Select → **D64 Room**)
- [x] Player start from thing type `1`
- [x] Use-activated door (special 1/31 family) — rising ceiling + hide opened faces; split 117/118
- [x] Texture hash → atlas (Carcosa stand-ins; WAD PNG first)
- [x] 2S upper/lower/mid (multi-span); stock thin map in `d64_room_door.mapwad`
- [x] Activation bits (use / walk / shoot) + key bits on action word
- [x] Sky flats `F_SKY*` / `F_SKYA`–`K` → outdoors; floors reach the horizon on open columns
- [x] Player radius 16 (stock D64) + min authored door width 48
- [x] Full Builder hand-export regression (open `d64_room_door.mapwad` / showcase PWADs in Builder, re-save, reload) — 2026-09-12
- [x] MACROS interpreter (useful subset; ref Doom64EX-Plus `p_macros.c`)
- [x] Scroll texture linedef / sector flags
- [x] EX+ embedded PNG `T_*` / `S_*` (CastleDoom harness)
- [x] Argument movers (floor/ceil/plat by/to `globalint`) + quake + artifact switches
- [x] Cameras / light-copy linedef specials (200/201/243, 205–209/222/234–235)
- [x] Sector/line property copy (218–223/230) + random line (240)
- [x] Thing misc (202/231/233/242/93/94/211/254)

**Self-contained custom WAD packaging:** EX+-style PNG lumps in `T_START`…`T_END` / `S_START`…`S_END`, plus optional `CARCOSA` / `CARCLUA`. Catalog PNGs remain a fallback for authoring.
