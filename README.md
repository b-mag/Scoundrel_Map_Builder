# Doom Builder 64 Enhanced (Carcosa / Scoundrel)

This is a **fork of [Doom Builder 64](https://github.com/styd051/Doom-Builder-64)** — itself a Doom Builder 2 variant by Kaiser, later maintained by Styd051 and Immorpher — used to author maps for **Carcosa** on **Scoundrel**.

Scoundrel is a **new, Android-first engine** (desktop for authoring and debug). It is not Doom 64 EX, not the N64 original, and not the 2020 remaster. Rendering, collision, atmosphere, and gameplay are its own: column raycaster, sector fog and weather, RPG systems. Maps saved here are meant to load in that engine.

## Compatibility (read this)

We started from Doom Builder 64’s map format so existing D64 geometry, lights, and things can round-trip. **That is a starting point, not a promise.**

This fork will keep adding Carcosa-only data (the `CARCOSA` / `CARCLUA` lumps, thing types 9200–9399, Sector Edit atmosphere, and more). Those extras, and any later format drift, **may or may not stay backward compatible** with stock Doom Builder 64, Doom 64 EX+, the remaster, or N64 toolchains.

- Maps **without** Carcosa lumps should still open in the parent editor in the usual way — until they don’t. Treat that as best-effort.
- Maps **with** Carcosa extras are for Scoundrel. Do not assume EX+ or the remaster will ignore them safely.
- Visuals in Scoundrel will not match EX-Plus or the remaster. Sky, fog, floors-to-horizon, and weather follow the Android renderer, not a port of the N64/PC software.

If you need a faithful Doom 64 mapper for EX+ / remaster / N64, use **upstream Doom Builder 64 Enhanced**, not this tree.

## What this editor is for

| | |
|--|--|
| **This repo** | Geometry, lights, things, scripts, and Carcosa atmosphere for Scoundrel |
| **Scoundrel** (`../scoundrel`) | Runtime: play, Android build, raycast/sector render |
| **Kotlin map builders** | Showcase PWADs until the WAD pipeline is the only path |

Longer notes live in [`docs_and_specs/`](docs_and_specs/): [VISION](docs_and_specs/VISION.md), [ROADMAP](docs_and_specs/ROADMAP.md), [CHANGELOG](docs_and_specs/CHANGELOG.md), [engine compatibility](docs_and_specs/ENGINE_COMPATIBILITY.md).

## Installation

No installer. Build (below) or copy a `Build\` tree and run `Builder.exe`. Replacing an older Doom Builder 64 folder in place is the usual upgrade; this fork’s configs and lumps are not a drop-in over upstream.

Requires **Microsoft .NET Framework 3.5**.

## Building

Windows only. Double-click `build.bat` (runs `scripts/build-builder.ps1`, `Release|x86`) or open `Builder.sln` in Visual Studio / Build Tools. Output: `Build\Builder.exe`.

Launch from `Build\` so SlimDX and the configuration files resolve.

Showcase maps: `../scoundrel/assets/maps/`. Marker lumps are **not** all `MAP01`:

| PWAD | Marker |
|------|--------|
| `d64_room_door.mapwad`, `carcosa_shoals.mapwad` | MAP01 |
| `carcosa_yhtill.mapwad` | MAP02 |
| `carcosa_demhe.mapwad` | MAP03 |
| `carcosa_diadem.mapwad` | MAP04 |

Picking the wrong marker yields “Unable to read the map data structures with the specified configuration.”

## Lineage

- **CodeImp** — Doom Builder 2
- **Kaiser (Samuel Villarreal)** — Doom Builder 64
- **Styd051, Immorpher, and contributors** — Doom Builder 64 Enhanced (the parent this repo was forked from)
- **This tree** — Carcosa / Scoundrel authoring; not an upstream tracking branch
