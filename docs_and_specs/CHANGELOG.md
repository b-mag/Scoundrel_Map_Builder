# Changelog — Doom Builder 64 Enhanced (Carcosa fork)

All notable changes to **this repository** (editor / configs / docs) go here.  
Engine-side changes live in the `scoundrel` repo; cross-link them when relevant.

## 2026-09-12 — Default to Carcosa game config only

### Configs
- Open Map / New Map default to `Doom64.cfg`, display name **Carcosa (Doom 64)**.
- Stock N64 / EX+ / Convert configs moved to `Build/Configurations/_archive/` so they no longer appear in the dropdown (Carcosa things + `CARCOSA`/`CARCLUA` live on the PC config only).

### Builder code
- `General.DEFAULT_GAME_CONFIG` + fallback selection in Open Map and Map Options when no `.dbs` preference exists (or it points at a removed config).

## 2026-09-12 — Builder-first multi-map authoring

### Builder code
- **Help → Carcosa: Multi-map WAD Tutorial** opens `Help/carcosa_multimap.html` in the browser (no CHM rebuild). Covers opening `carcosa.wad`, switching `MAP##`, NPCs/Lua/zones, cross-map flags, and creating a megawad with **Save Map Into**.
- **Tools → Pack Maps into WAD…** merges single-map `.wad` / `.mapwad` files into one multi-map PWAD (optional `CARCWLD` from a `.lua` on the first map). Same job as Scoundrel `:game:mergeCarcosaWad`, usable without leaving the editor.
- `build-builder.ps1` copies the Carcosa help HTML + `default.css` into `Build/Help/`.

### Authoring model
- Day-to-day source of truth: one multi-map WAD; **Save Map** is the build. No project type. Gradle merge remains CLI/CI optional.

### Related engine docs (`scoundrel`)
- `docs/carcosa/LUA_SCRIPTING.md` / `CARCOSA_LUMP.md` aligned to megawad-canonical workflow.

## 2026-09-12 — Open-world zones + world script + richer Lua stubs

### Builder code
- Sector Edit **Zone** dropdown writes `CarcosaAmbient` (atmosphere zone id for shared rain policies). Multi-select applies the same zone to many sectors.
- **Edit Lua** stubs teach cross-map `flag` / `open_tag` patterns.
- `CARCWLD` map lump (optional world script tab; same Lua cfg as CARCLUA).

### Configs
- `Carcosa_Lua.cfg`: `on_region`, `npc_at`, `move_npc`, `npc_alive`, `zone_weather`; flag help notes cross-map persistence.

### Related engine work (`scoundrel`)
- One `carcosa.wad` with MAP01–04; pack `world.lua` + `zones.json`; WorldState NPC roster + zone rolls; cross-map flag sample (aldon_slain → Shoals crypt).

## 2026-09-12 — Thing Edit Script / RPG + CARCLUA jump

### Builder code
- Thing Edit: **Script / RPG** group writes CARCOSA thing extras (archetype, initial state, flags, dialogue fragment, patrol, quest flag).
- **Edit Lua for this thing** opens the Script Editor on the map `CARCLUA` lump and jumps to an existing `thing_type` match, or inserts an `on_use` stub. Scripts stay map-level (one lump; things addressed by type / runtime id).
- Linedef Identification: hint that `Tag` + `carcosa.open_tag(n)` opens matching doors.

### Configs
- `Carcosa_things.cfg`: `width` / `height` are editor view sizes; runtime billboard size is pack `beasts.json` `scale`.

### Related engine work (`scoundrel`)
- Gradle `:engine` / `:game` split. Runtime maps load only from pack PWADs. JUnit baseline for lumps, Lua sandbox, doors, sky, catalog scale.

## 2026-09-12 — Carcosa sector atmosphere + one-click build

### Configs (additive)
- Waygates 9240–9243: Builder thing size **47×111** (was 32×96) so they match the engine billboard (+15 map units).
- `CARCOSA` lump remains optional `blindcopy`; the editor now also **parses and rewrites** it so extras survive sector insert/delete.

### Builder code
- `CarcosaLumpIO` reads/writes the sidecar on map open/save (`MapManager`).
- Sector Edit: **Carcosa atmosphere** group — weather, mood, fog density/start/max, fog RGB, outdoors, place name. Defaults write nothing; stock maps without the lump stay valid. Thing extras round-trip with no extra UI.
- `build.bat` + `scripts/build-builder.ps1` — prefers VS MSBuild that ships Roslyn (VS 18 Build Tools on this machine); output `Build\Builder.exe`. First launch copies `Builder64.default.cfg` so a missing user config no longer crashes.

### Related engine work (`scoundrel`)
- Sky/horizon: floors meet the sky on open columns; `F_SKYA`–`K` recognized; façade-only wall cap.
- CARCOSA fog no longer overwritten by placeName lighting; rain/fog draws in the half-res FBO.

### Regression
- Double-click `build.bat`, then open `scoundrel/assets/maps/d64_room_door.mapwad` (`MAP01`) and re-save (lump/struct sizes must still match `Doom64MapSetIO`).
- Open `scoundrel/assets/maps/carcosa_shoals.mapwad` (`MAP01`) — Sector Edit should show Carcosa extras; `CARCLUA` tab still present. Re-save and play in Scoundrel.
- Yhtill / Demhe / Diadem are `MAP02` / `MAP03` / `MAP04`. Opening them as `MAP01` fails with “Unable to read the map data structures” (no VERTEXES under that marker).
- Headless round-trip: from `Build\`, `Builder.exe -NOSETTINGS -DELAYWINDOW -SAVETHENEXIT -CFG Doom64.cfg -MAP MAP01 path\to\map.wad` (use the map’s actual `MAP##`).

## 2026-09-11 (c) — Extension 1b: Ringwarden quest authoring

### Configs (additive)
- Extended `Carcosa_things.cfg`:
  - `9222` Ringwarden (white-masked NPC) — `Carcosa NPCs`.
  - `9270` Jeweled Ring pickup, `9271` Obsidian Necklace, `9272` Map Fragment — new `Carcosa Items` category.
- Extended `Carcosa_Lua.cfg` keyword help: `on_choice`, `ask`, `has_item`, `give_item`, `take_item`, `hide`. Still **no** `compiler` / `resultlump`.

### Builder code
- None. Script Editor + `CARCLUA` tab remain the authoring UI.

### Related engine work (`scoundrel`)
- Ringwarden dialogue quest in Pallid Shoals `CARCLUA` (YES/NO gift of necklace + map fragment).
- Script API: satchel items, `carcosa.ask` → HUD choices → `on_choice`, `carcosa.hide`.
- Ambient beats: Sethmarr chime on enter; distant tide-bell on a timer while Oshkerith lives.

### Regression
- Open `scoundrel/assets/maps/carcosa_shoals.mapwad` → Script Editor `CARCLUA` tab shows the Ringwarden script.
- Place `9222` / `9270` from the Things browser; save and re-open without losing `CARCLUA`.

## 2026-09-11 (b) — Extension 1: Lua event scripting

### Configs (additive)
- Added `Build/Scripting/Carcosa_Lua.cfg` — script configuration for Carcosa Lua event scripts. Lua lexer (`15`), keyword help for the six engine event entry points and the whole `carcosa.*` host API. Declares **no** `compiler` / `parameters` / `resultlump`: Lua is not compiled, the lump is the payload, and declaring a result lump would make Builder overwrite another lump with compiler output.
- Added optional `CARCLUA` lump to `D64_misc.cfg` `doommaplumpnames` with `script = "Carcosa_Lua.cfg"`. That gives it a Script Editor tab and gets it preserved by `MapManager.CopyLumpsByType(..., copyscript: true)` on save. Maps without the lump are unchanged.
- Extended `Carcosa_things.cfg`:
  - `9221` Enkai (flask keeper) — new `Carcosa NPCs` category.
  - `9240–9243` waygates, one per destination region — new `Carcosa Travel` category. These existed in the engine but had never been declared here, so they could not be placed in the editor.
  - `9250` Lum Fountain (save / purify).
  - `9260–9263` Lum rations — new `Carcosa Rations` category.

### Builder code
- Still no C# / IO format changes. Script-lump support was already generic; only configuration was needed.

### Related engine work (`scoundrel`)
- `CARCLUA` read/write: `Doom64MapReader.Doom64Map.luaScript`, `Doom64MapWriter.writeFlatPwad(..., lua)`, `SectorWorld.luaScript`.
- Sandboxed LuaJ VM (`LuaScriptHost`) with six event hooks and a 15-function host API; `io`/`os`/`package`/`require`/`load*` all removed.
- Lum fountains: purify with the crystal flask, save point, HP/MP refill, ordinary-enemy respawn, and the exclusive place to attune primary/secondary spells.
- Crystal flask (Estus-style) with HUD icon, fatigue meter that scales attack strength, and ration buffs.
- New spec: `scoundrel/docs/carcosa/LUA_SCRIPTING.md`.

### Regression
- `./gradlew :core:checkCarcosaSystems` parses this config's real `doommaplumpnames` block and asserts every exported showcase map uses only declared lumps, that all required lumps are present, and that every thing type the world builder places is declared here. It also asserts `Carcosa_Lua.cfg` never grows a `compiler` or `resultlump`.
- Open and re-save `scoundrel/assets/maps/carcosa_shoals.mapwad`; the `CARCLUA` tab must show the Pallid Shoals script and survive the round trip.

## 2026-09-11

### Configs (additive)
- Added `Build/Configurations/Includes/Carcosa_things.cfg` — thing types **9200–9399** (divine beasts, husks, Enkhari vessels, tablet, cairn, torch). Does not remap stock or EX+ IDs.
- Included Carcosa things from `Doom64.cfg` `thingtypes` block after `D64_things.cfg`.
- Added optional `CARCOSA` lump to `D64_misc.cfg` `doommaplumpnames` with `blindcopy = true` so Kotlin-exported sidecars survive open/save round-trips. Stock maps without the lump are unchanged.

### Builder code
- Still no C# / IO format changes. Sector fog fields remain engine-authored via the CARCOSA lump (no binary sector struct expansion yet).

### Related engine work (`scoundrel`)
- Showcase biomes export: `carcosa_shoals/yhtill/demhe/diadem.mapwad`.
- Full RPG pillars: magic affinities, spatial audio, behavior trees, weighty melee, region manager, Ingathering.

### Regression
- Open and re-save an existing stock Doom 64 map; confirm lump/struct sizes still match `Doom64MapSetIO`. CARCOSA absence must not error.

## 2026-09-10

### Docs
- Added `docs_and_specs/` (this tree). Upstream root `README.md` unchanged.
- Recorded engine compatibility audit vs `Doom64MapSetIO` / stock D64 lumps.
- Documented Step 1 (engine format parity) and Step 2 (weather / scripting / quests) roadmap.
- Linked `../scoundrel/FUTURE_WORK.md` as the home for engine work not yet supported in the Builder; Step 2 stays additive so existing D64 content keeps working.

### Builder code
- *(none yet — fork baseline only)*

### Related engine work (`scoundrel`)
- Aligned `Doom64MapReader` with Builder layouts: linedef `flags:u32`, sidedef hi/lo/mid, lights 6-byte+tag, flat `MAP##` PWAD load.
- Sector bounds rebuild + D64 light index convention (`<256` gray / `≥256` table).
- On-screen FPS counter in play HUD for performance monitoring.
