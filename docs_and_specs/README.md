# Scoundrel / Carcosa — Doom Builder 64 docs & specs

This folder tracks **this fork’s changes and Scoundrel integration**.  
The repo-root `README.md` is the product pitch (fork lineage + Android engine; compatibility not guaranteed).

| Doc | Purpose |
|-----|---------|
| [CHANGELOG.md](CHANGELOG.md) | Every change we make to this Builder repo |
| [ENGINE_COMPATIBILITY.md](ENGINE_COMPATIBILITY.md) | Scoundrel vs stock Doom 64 / Builder map format |
| [ROADMAP.md](ROADMAP.md) | Step 1 engine gaps → Step 2 Builder RPG extensions |
| [VISION.md](VISION.md) | Product north star (King’s Field / Daggerfall / Carcosa) |
| [../Help/carcosa_multimap.html](../Help/carcosa_multimap.html) | In-editor tutorial: multi-map Carcosa WAD authoring (also Help menu) |

**Engine repo:** `../scoundrel` (sibling under `carcosa/`)  
**Map path (target):** Doom Builder 64 → flat multi-map PWAD (`MAP##` + lumps) → `Doom64MapReader` → `SectorWorld`  
**Day-to-day:** edit `scoundrel/assets/maps/carcosa.wad` in Builder and Save. Optional pack: Tools → Pack Maps into WAD… / `gradlew :game:mergeCarcosaWad`.
