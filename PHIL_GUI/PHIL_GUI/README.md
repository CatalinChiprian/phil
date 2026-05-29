PHIL_GUI
========

Lightweight GUI for PHIL (Pipetting Helper Imaging Lid) built with Avalonia and .NET 9.

Summary
-------
This repository contains the Avalonia-based desktop UI for controlling and calibrating the PHIL instrument: plate/well visualization, calibration workflow, pump controls, and Action scheduling.

Key technologies
---------------
- .NET 9
- Avalonia UI
- CommunityToolkit.Mvvm (MVVM helpers)

Requirements
------------
- .NET 9 SDK
- Visual Studio 2022/2026 or VS Code
- (Windows) Serial port access for hardware communication when testing with a device

Build & run
-----------
1. Open the solution in Visual Studio or use the dotnet CLI.
2. Restore and build:
   dotnet restore
   dotnet build
3. Run the app from the IDE or with:
   dotnet run --project PHIL_GUI/PHIL_GUI.csproj

Alternatively, open the project in Visual Studio Code and press Run (or use the built-in debugger) to start the application.

Configuration
-------------
- Application settings live under Models/Settings and are loaded at startup. Settings can be changed at runtime via the app's settings UI; changes take effect immediately (no restart required).

Project layout (high level)
---------------------------
- PHIL_GUI/Views — Avalonia XAML views (MainWindow, Calibration, etc.)
- PHIL_GUI/ViewModels — View model classes (MVVM)
- PHIL_GUI/Models — Domain models (CalibrationPoint, Well, Settings)
- PHIL_GUI/Services — Services for serial communication, robot protocol, scheduling

Notes & recommendations
-----------------------
- Keep long-running or blocking I/O off the UI thread (use async/await).
- Use UI view-model wrappers (eg: ActionItem) when you need transactional edits or validation.
- When introducing new KeyBindings in AppKeyBindings make sure to add the KeyBindings in the specific Window axaml.The Window axaml.cs must subscribe to PropertyChanged event of AppKJeyBindings and update KeyBindings everytime.

Contributing
------------
- Fork the repo, create a feature branch, and submit a PR. Follow existing code style and include tests for new logic where feasible.

License
-------
See repository root for license details (if present). If none, contact the project owner.

Contact
-------
Refer to the project repository and issue tracker for questions and bug reports.