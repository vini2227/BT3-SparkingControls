# BT3 Sparking Controls 1.1 — Linux x64

A Sparking! ZERO-inspired control layout and HUD for the native Linux version of Tenkaichi3Decomp. This is the Linux port. Read FEATURES-EN.md for controls, differences and limitations.

## Linux 1.1 — Changes

- Ported Story / Mission 100 fight detection for the reviewed original Linux Decomp, including native R3 story confirmation handling.
- Cropped CPU composition and menu snapshots; reused the X11 image and transfer buffer instead of allocating/destroying them per frame. Idle HUD is unmapped; unchanged frames are not uploaded again. Game viewport coordinates and fade timings are retained.
- Added Screen edges placement, persisted with the other three layouts and included in the F8 cycle and translated launcher.
- Cached HD boost-icon ink bounds. Diagnostic launcher logging is off by default; use `bash start.sh --diagnostics` only when collecting a report. Settings stay in the existing XDG directory.
- Rebuilt the native ELF library and Mono frontend. The ELF compatibility resolver still targets reviewed official v0.1.37/v0.1.38.1; automatic Windows variant detection and Windows-specific debugger/heap/arena changes are not included. No universal future-version or updater support is claimed.

Validation: 446,473 emulated packet/ABI/native-command checks and 9 loader checks for EACH reviewed Linux executable; 192 cropped pixel/persistence checks; 180 fade, 693 responsive, 1,408 card, 565 split-screen, 93 shortcut and 2,282 launcher checks. UI tests ran on Windows .NET, not Mono. Real Linux LD_PRELOAD/X11 presentation, native Wayland, physical controllers and online rollback remain unverified. This is not a confirmed fix for the reported Windows Mission 100 ×8/texture-pack slowdown.

Special thanks to **Aero** for extensive testing, reproduction details and diagnostic logs that guided the performance work.

## Retained features

Vertical Z Dash on R2 + L1/L2; legacy double taps disabled. Releasing Circle while holding R1 waits for native stance exit and resumes guard without another R1 press. All battle menus use 100 ms fade-in / 80 ms fade-out, with immediate dirty-frame presentation on Linux. Native BT3 animation windows still apply. Read CHANGELOG.md and the Controls and Setup Guide.

## Start

Extract the entire ZIP into one folder. Keep the executable, .so library, profiles, compatibility files and assets together. Run `bash start.sh`, choose the native Linux game executable, then Save and start. The managed `.exe` runs with Mono, without Wine or Proton. The add-on must launch the game; do not separately launch another copy.

Requires Linux x86-64, Mono with Windows Forms/System.Web.Extensions, libgdiplus, fonts for the selected language and a composited X11 display or working XWayland. Native Wayland overlays are not implemented. The library was cross-compiled with a glibc 2.35 target. On a different system, rebuilding from source may be necessary.

## Compatibility and settings

Official Linux v0.1.37 and v0.1.38.1 executables passed the compatibility checks. The symbol resolver can accept compatible address changes after verifying 61 complete functions and references. Structural changes or stripped symbols can require a new reviewed profile. Compatibility is not guaranteed for every release.

Settings persist in `$XDG_DATA_HOME/BT3-SparkingControls/linux` (default `~/.local/share/BT3-SparkingControls/linux`). Runtime cache is in the sibling `linux-runtime` folder. Prior `linux-online-v0.1.37` settings are migrated when applicable. Updating the extracted application does not reset saved settings.

F8: placement. F7: attack frame. F6: attack visibility. Rebind/disable/reset shortcuts in the launcher. In-game changes persist. Enable the shortcut lock to prevent battle-time changes.

## Build and diagnostics

Sources and AGPL-3.0 license are included. With a C compiler (clang recommended), Mono development tools/mcs and Python 3 installed, run `bash build.sh`. Rebuilding the native library refreshes its profile hashes. Reviewed compatibility metadata is already included; do not regenerate it merely to bypass a compatibility error.

`bash start.sh --check /path/to/Tenkaichi3Decomp` validates the game/library without launching gameplay. Maintainer metadata regeneration: `python3 tools/build-compatibility-linux.py --game /path/to/reviewed/game --output compatibility-linux.json` (requires tests/requirements.txt). Test scripts accept the game/resolved-profile options documented by `--help`/their argument parser; CPU emulation is not an online test.

## Validation limits

446,473 CPU-emulated packet/ABI/native-command checks and 9 emulated loader checks passed for EACH of v0.1.37 and v0.1.38.1. Managed UI checks: 2,282 launcher checks across 16 languages, 93 shortcut checks, 693 responsive checks and 12 preview states. Incompatible modified ELF code and Windows executables were refused.

The UI fixtures ran under Windows .NET, not Mono. Real Linux LD_PRELOAD, X11 fullscreen/compositing, physical-controller timing, gameplay and online rollback have NOT been tested here. No universal no-desync guarantee is made. No game executable, game artwork or HD texture pack is bundled.
