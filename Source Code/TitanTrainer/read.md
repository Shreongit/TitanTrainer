# Titan Trainer

Titan Trainer — modular, production-oriented GTA V single-player trainer built with ScriptHookVDotNet and LemonUI.

## Installation

1. Build the project in Rider (Target: .NET Framework 4.8) to produce `TitanTrainer.dll`.
2. Copy `TitanTrainer.dll` to GTA V's `scripts` folder.
3. Copy `ScriptHookVDotNet.dll` and `LemonUI.dll` into the same `scripts` folder or provide them via `libs\` during development.
4. Ensure ScriptHookV is installed in the GTA V directory.
5. Start GTA V (recommended Build 3889).

## Requirements

- GTA V (Single Player)
- ScriptHookV
- ScriptHookVDotNet (v3.7 nightly recommended)
- LemonUI (menu rendering)
- .NET Framework 4.8
- Newtonsoft.Json (NuGet)

## Controls

- F5 — Toggle Titan Trainer main menu (default; configurable at Data/config.json)

## Architecture

- Core: ScriptMain.cs, Config.cs, Logger.cs (startup, config, logging)
- UI: LemonUI-based menus (MenuManager, MainMenu, VehicleMenu, SettingsMenu)
- Vehicles: VehicleDatabase (JSON), VehicleSpawner (model + spawn logic), VehicleManager (favorites/history), VehicleFactory
- Player: PlayerManager (player utilities)
- World: WeatherManager, TimeManager
- Utils: ServiceContainer, NotificationService, ThemeManager, FileUtils
- Data: JSON data files (vehicles.json, favorites.json, config.json)

## Folder explanation

- Core: Initialization, DI container, logger and config storage.
- UI: Menu controllers and UI glue.
- Vehicles: Vehicle data, spawn logic and persistence.
- Utils: Reusable helpers and services.
- Data: JSON datasets and user-saved files.

## Roadmap

- [x] Stage 1: startup/configuration/logging and disposal reliability.
- [x] Stage 2: player stamina, vehicle spawning feedback, spawn upgrades, rainbow effects, and vehicle flag cleanup.

- Vehicle preview pane (3D preview)
- Advanced spawn options UI (colors, plates, xenon, neon, mods)
- Instant search with suggestions
- ThemeManager + LemonUI theming
- VersionChecker + plugin metadata
- Community vehicles.json import/export

## Developer notes

- JSON files load once at startup. Update Data/vehicles.json and restart the game to apply changes.
- Logs are written to Logs/TitanTrainer/YYYY-MM-DD.log.

## Stage 3 — Vehicle category navigation

The vehicle browser is now split into separate menus:

1. **Vehicle Spawner** — existing vehicle controls plus `Browse Vehicles by Category`.
2. **Vehicle Categories** — Super, Sports, Bikes, Trailers, Trucks, Planes, Boats, and other categories present in `Data/vehicles.json`.
3. **Category submenu** — only vehicles assigned to the selected category, with a `Back` item to return to the category list.

The browser menus are registered with LemonUI's object pool. The catalogue includes additional common truck and trailer model entries. If a model is unavailable in the installed GTA V build, spawning may fail; trailer models may also spawn unattached and need a tow vehicle. Categories with no available entries remain visible and show an explanatory placeholder.



## Stage 4 — Expanded Trainer Features

This release expands Titan Trainer with practical Story Mode trainer tools inspired by common single-player trainer workflows (it is not a feature-for-feature clone of Menyoo).

- **Weapons & Loadouts:** equip common weapons, grant a standard arsenal, refill current-weapon ammo, toggle infinite clip, and remove all weapons.
- **Teleport:** waypoint, forward/safe/sky movement, and curated Los Santos/Blaine County destinations.
- **Vehicle Doors & Windows:** toggle individual doors, open/close all doors, and roll windows up/down.
- **Waypoint fix:** removed a faulty repeated-teleport loop; the player is now moved once to a point above the waypoint.
- **Vehicle control cleanup:** resets the windscreen-ejection setting when the player is no longer in a vehicle.

### Compatibility and testing

Target framework remains .NET Framework 4.8 for ScriptHookVDotNet 3. The source was statically reviewed and project/data files were validated in this packaging environment, but compilation and in-game behavior must still be verified locally with the matching ScriptHookVDotNet/LemonUI versions. Use only in GTA V Story Mode/offline; do not use trainer functions in GTA Online.

## Stage 5 — Entity Spawner

Adds a dedicated **Entity Spawner** entry to the main menu, with separate **Prop Spawner** and **Pedestrian Spawner** submenus. Each submenu has explicit back navigation. The curated lists include common props (barrels, crates, cones, barriers, benches, chairs, umbrellas, tables, briefcases and pallets) and pedestrian archetypes (beachgoer, business people, tourist, hiker, skater, police officer, paramedic, firefighter and mechanic).

- Props and pedestrians spawn a short distance in front of the player.
- Model names are validated and requested before spawning; failed model loads produce a notification instead of attempting creation.
- Pedestrians spawned by the trainer are made persistent and have ambient events blocked so they do not immediately wander off.
- Each submenu can remove the most recently spawned prop or pedestrian created by Titan Trainer.
- Entity creation and cleanup are guarded with exception logging and user-facing feedback.

### Stage 5 validation

The archive and source structure can be checked here, but this environment does not have the .NET Framework/MSBuild toolchain needed to compile against your local ScriptHookVDotNet and LemonUI assemblies. Build in Rider and test each model in GTA V Story Mode before treating the stage as release-ready. The prop/ped lists are curated starting points; model availability can vary with GTA V builds and installed DLC.
