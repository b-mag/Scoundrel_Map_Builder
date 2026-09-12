# Scoundrel / Carcosa — Doom Builder 64 docs & specs

This folder tracks **this fork’s changes and Scoundrel integration**.  
The repo-root `README.md` is the product pitch (fork lineage + Android engine; compatibility not guaranteed).

| Doc | Purpose |
|-----|---------|
| [CHANGELOG.md](CHANGELOG.md) | Every change we make to this Builder repo |
| [ENGINE_COMPATIBILITY.md](ENGINE_COMPATIBILITY.md) | Scoundrel vs stock Doom 64 / Builder map format |
| [ROADMAP.md](ROADMAP.md) | Step 1 engine gaps → Step 2 Builder RPG extensions |
| [VISION.md](VISION.md) | Product north star (King’s Field / Daggerfall / Carcosa) |

**Engine repo:** `../scoundrel` (sibling under `carcosa/`)  
**Map path (target):** Doom Builder 64 → flat PWAD (`MAP##` + lumps) → `Doom64MapReader` → `SectorWorld`
