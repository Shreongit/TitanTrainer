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