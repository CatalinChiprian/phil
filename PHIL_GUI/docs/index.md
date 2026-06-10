---
_layout: landing
_disableToc: true
_disableContribution: true
_customCss: styles/custom.css
---

# PHIL Robot Control System

**P**ipetting **H**andling **I**ntegrated **L**aboratory - An automated liquid handling platform for 96-well plates and organ-on-chip devices.

---

## Quick Links

<div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin: 30px 0;">
  <div style="border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
    <h3>Getting Started</h3>
    <p>New to PHIL? Start here!</p>
    <a href="getting-started.html">Read Guide ?</a>
  </div>

  <div style="border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
    <h3>API Reference</h3>
    <p>Browse the code documentation</p>
    <a href="../api/PHIL_GUI.html">View API ?</a>
  </div>

  <div style="border: 1px solid #ddd; padding: 20px; border-radius: 8px;">
    <h3>Downloads</h3>
    <p>PDF manuals and reports</p>
    <a href="resources/">View Resources ?</a>
  </div>
</div>

---

## Documentation

| Resource | Description |
|----------|-------------|
| [User Handbook](resources/PHIL-Handbook.pdf) | Complete user guide (PDF) |
| [Technical Report](resources/Technical-ReportANT.pdf) | Technical specifications (PDF) |

---

## Features

-   **96-Well Plate Support** - Standard 8x12 grid layout
-   **Organ-on-Chip Mode** - 4x6 paired well configuration  
-   **Action Scheduling** - Automated medium exchange
-   **Manual Control** - Direct XYZ movement and pump control
-   **Visual Calibration** - Interactive well mapping
-   **Multi-Pump Support** - Up to 4 independent pumps

---

## Architecture

```
PHIL_GUI/
	Models/          # Data structures (wells, actions, robot state)
	ViewModels/      # MVVM presentation logic
	Views/           # Avalonia UI (XAML)
	Services/        # Serial communication & settings
```

---

## Contributing

This project is built with .NET 9 and Avalonia UI. Contributions are welcome!

- **Repository**: [github.com/CatalinChiprian/phil](https://github.com/CatalinChiprian/phil)
- **Issues**: Report bugs or request features
- **Pull Requests**: Submit improvements

---
