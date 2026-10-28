<div align="center">

# 🎮 TITAN TRAINER

### Titan Trainer: A Single-Player Trainer for GTA V Legacy

**A hierarchical, menu-driven trainer written in C# on .NET Framework 4.8.**

*Understand the scripting layer. Build the menu. Learn how GTA V mods work under the hood.*

<br>

![Status](https://img.shields.io/badge/STATUS-IN%20DEVELOPMENT-orange?style=for-the-badge)
![C#](https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white)
![.NET Framework](https://img.shields.io/badge/.NET%20FRAMEWORK-4.8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ScriptHookVDotNet](https://img.shields.io/badge/SHVDN-V3-1f6feb?style=for-the-badge)
![LemonUI](https://img.shields.io/badge/LEMONUI-SHVDN3-2ea043?style=for-the-badge)
![Game Build](https://img.shields.io/badge/GTA%20V%20LEGACY-V3889-red?style=for-the-badge)

<br>

[Overview](#-what-is-titan-trainer) • [Architecture](#-architecture) • [Features](#-features) • [Tech Stack](#-tech-stack) • [Building](#-building-from-source) • [Installation](#-installation) • [Principles](#-design-principles) • [License](#-license)

</div>

---

## 🧭 What Is Titan Trainer?

**Titan Trainer** is a single-player trainer for **Grand Theft Auto V Legacy**, written entirely in **C# targeting .NET Framework 4.8**. It runs as a ScriptHookVDotNet script and exposes player, vehicle, entity, weapon and world controls through a nested, hierarchical in-game menu built on **LemonUI**.

The project focuses on clean structure: every feature area is its own module, menus are composed rather than hard-coded into one giant file, and game interaction goes through the managed ScriptHookVDotNet API wherever possible.

> [!NOTE]
> Titan Trainer is intended for **Story Mode only**. It is not designed for, tested with, or supported in GTA Online.

---

## 🏗️ Architecture

Titan Trainer is built as a single managed class library (`.dll`) that ScriptHookVDotNet loads from the `scripts` folder. At a high level:

```
┌─────────────────────────────────────────────┐
│               GTA V Legacy (V3889)          │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│              Script Hook V (native)         │
└──────────────────────┬──────────────────────┘
                       │
┌──────────────────────▼──────────────────────┐
│     ScriptHookVDotNet v3 (managed runtime)  │
└──────────────────────┬──────────────────────┘
                       │  loads scripts/*.dll
┌──────────────────────▼──────────────────────┐
│               Titan Trainer (C#)            │
│  ┌───────────────────────────────────────┐  │
│  │  Script entry point (Tick / KeyDown)  │  │
│  └──────────────┬────────────────────────┘  │
│                 │                           │
│  ┌──────────────▼────────────────────────┐  │
│  │  Menu layer (LemonUI: pool + menus)   │  │
│  └──────────────┬────────────────────────┘  │
│                 │                           │
│  ┌──────┬───────┴──┬──────────┬──────────┐  │
│  │Player│ Vehicles │ Entities │ World /  │  │
│  │      │          │          │ Weapons  │  │
│  └──────┴──────────┴──────────┴──────────┘  │
└─────────────────────────────────────────────┘
```

### Core Components

| Component | Responsibility |
|---|---|
| **Script entry point** | Inherits from `GTA.Script`, handles the `Tick` and `KeyDown` events, and opens or closes the menu with the configured key (F5). |
| **Menu layer** | Owns the LemonUI `ObjectPool` and the main menu, and processes the pool every tick so menus draw and receive input. |
| **Feature modules** | Each area (Player, Vehicles, Entities, Weapons and World) builds its own submenu and attaches it to the main menu. |
| **State handling** | Toggle options (for example, persistent effects) are applied in the tick loop and cleaned up when switched off. |

> [!TIP]
> Keeping each feature area in its own module means a new category can be added by creating a submenu and attaching it to the main menu, without touching existing features.

---

## ✨ Features

### 🧍 Player Options
- Health and armor controls
- Movement abilities and player effects
- Wanted-level management

### 🚗 Vehicle System
- Vehicle spawning with category browsing
- Vehicle repair and handling controls
- Vehicle customization and performance upgrades
- Wheels, colors, windows, bumpers, lights and liveries
- Vehicle search, favorites and presets, where supported

### 🧩 Entity Spawner
- Pedestrian and prop spawning
- Entity placement and management
- Supported pedestrian behavior controls

### 🔫 Weapons and World
- Weapon options for managing your loadout
- Weather presets and environment controls
- Time adjustments
- Teleportation and supported saved locations
- Scene management, where supported

### 🗂️ Menu Navigation
- Nested, hierarchical menu design
- Enter submenus from the parent menu, return with the configured back control
- Default menu key: **F5**

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Language** | C# |
| **Framework** | .NET Framework 4.8 |
| **Scripting API** | [ScriptHookVDotNet v3](https://github.com/scripthookvdotnet/scripthookvdotnet) |
| **Native hook** | [Script Hook V](http://www.dev-c.com/gtav/scripthookv/) |
| **Menu UI** | [LemonUI (SHVDN3)](https://github.com/LemonUIbyLemon/LemonUI) |
| **IDE** | Visual Studio 2022 |
| **Target game** | Grand Theft Auto V Legacy, V3889 |

### Requirements
- Grand Theft Auto V Legacy
- Script Hook V
- ScriptHookVDotNet
- LemonUI SHVDN3

---

## 🔨 Building From Source

### Prerequisites
- Visual Studio 2022 (or newer) with the **.NET desktop development** workload
- **.NET Framework 4.8 Developer Pack**
- A copy of GTA V Legacy with Script Hook V and ScriptHookVDotNet installed

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/ifshreyas/TitanTrainer.git
   cd TitanTrainer
   ```

2. **Open the solution** in Visual Studio.

3. **Set the target framework** to `.NET Framework 4.8` in the project properties.

4. **Add references**
   - `ScriptHookVDotNet3.dll` from your GTA V directory
   - `LemonUI.SHVDN3.dll` from the LemonUI release

   Set both to **Copy Local = False** for ScriptHookVDotNet, since the game already loads it.

5. **Build** in `Release` configuration.

6. **Copy the output** `.dll` (and `LemonUI.SHVDN3.dll`) into the `scripts` folder of your GTA V directory.

> [!IMPORTANT]
> The ScriptHookVDotNet version you reference at build time must match the version installed in the game. A mismatch is the most common cause of a script failing to load.

---

## 📦 Installation

1. Install compatible versions of the required dependencies from their official sources.
2. Place the trainer files in the `scripts` folder of your GTA V directory. Create the folder if it does not exist.
3. Launch GTA V Legacy and enter **Story Mode**.
4. Press **F5** to open the trainer menu.

---

## ✅ Compatibility

| Item | Version |
|---|---|
| **Game** | Grand Theft Auto V Legacy, V3889 (The Kortz Center Heist Update) |
| **Runtime** | .NET Framework 4.8 |
| **ScriptHookVDotNet** | v3 |
| **LemonUI** | SHVDN3 build |

**Known limitations**
- Some vehicle and pedestrian features depend on the model in use, so certain options may not apply to every vehicle or entity.
- After a game update, the trainer may stop loading until Script Hook V and ScriptHookVDotNet are updated.

If you find a bug, please open an issue with your game version, dependency versions, and the relevant lines from `ScriptHookVDotNet.log`.

---

## 🧱 Design Principles

- **Modular by default.** Each feature area owns its own submenu and logic.
- **Managed API first.** Use ScriptHookVDotNet wrappers where they exist, and only reach for lower-level calls when needed.
- **Cheap tick loop.** Only active toggles do work each frame; everything else stays idle.
- **Clean up after yourself.** Spawned entities and applied effects are released or reverted when disabled.
- **Single-player only.** No online functionality, by design.

---

## 🙏 Credits

- **Script Hook V** by Alexander Blade
- **ScriptHookVDotNet** by crosire and contributors
- **LemonUI** by Lemon and contributors

All third-party libraries remain the property of their respective authors and are used under their original licences. No third-party files are redistributed in this repository.

---

## ⚠️ Disclaimer

Titan Trainer is intended for **single-player use only**. Do not use it in GTA Online or any multiplayer mode. Use at your own risk and back up your save files before installing any mod. This project is not affiliated with or endorsed by Rockstar Games or Take-Two Interactive.

---

## 📄 License

**All Rights Reserved.** Copyright © 2026 Shreyas Pathak.

The source code is published for viewing and learning. Redistribution or modification requires prior written permission from the author.

---

<div align="center">

**Built by [Shreyas Pathak](https://github.com/ifshreyas)**

</div>
