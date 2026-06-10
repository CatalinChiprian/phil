# Getting Started

## Prerequisites

- .NET 9 SDK
- Visual Studio 2026 or later
- Serial port access for robot communication

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/CatalinChiprian/phil
   ```

2. Open `PHIL_GUI.sln` in Visual Studio

3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

4. Build the solution:
   ```bash
   dotnet build
   ```

## First Run

1. Launch the application
2. Select the serial port for your PHIL robot
3. Click **Connect**
4. The main window will open after successful connection

## Configuration

### Plate Type Selection

Navigate to **Settings** → **Plate** to choose between:
- **96-Well Plate** (8×12 grid)
- **Organ-on-Chip Plate** (4×6 pairs)

### Keyboard Shortcuts

Customize keyboard shortcuts in **Settings** → **Controls**

## Further Reading

- [📘 PHIL Handbook (PDF)](PHIL-Handbook.pdf) - Complete user guide
- [📄 Technical Report ANT (PDF)](Technical-ReportANT.pdf) - Technical specifications
- [API Documentation](api/index.html) - Code reference
