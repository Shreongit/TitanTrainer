# Titan Trainer — Stage 1 fixes

This pass focuses on core startup, configuration, logging, and shutdown reliability.

## Changes

- Read `ToggleMenuKey` from `Data/config.json` instead of hard-coding F5.
- Fall back to F5 and repair invalid/blank hotkey configuration values.
- Guard tick and keyboard handlers until initialization has completed and after disposal starts.
- Make disposal idempotent and remove the vehicle manager update call from shutdown.
- Hide all trainer menus during menu-manager disposal.
- Ensure a fallback configuration is saved when configuration loading fails.
- Prevent logger initialization failures from recursively re-entering logger initialization.
- Replace the empty `favorites.json` placeholder with valid JSON (`[]`).
- Add a `.gitignore` for build output, IDE files, and runtime logs.

## Validation

- JSON data files and the project file should be validated locally.
- A .NET Framework 4.8 build and in-game ScriptHookVDotNet test are still required. The current execution environment does not have the .NET CLI/MSBuild installed, so this pass does not claim a successful compiled build.
